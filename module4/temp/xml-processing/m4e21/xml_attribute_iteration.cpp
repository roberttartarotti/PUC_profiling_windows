#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <msxml6.h>
#include <comdef.h>

static const GUID ProviderGuid = 
{ 0xA3345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE7 } };

REGHANDLE hProvider = 0;

void LogEvent(PCWSTR message)
{
    if (hProvider)
    {
        EVENT_DATA_DESCRIPTOR eventData[1];
        EventDataDescCreate(&eventData[0], message, (ULONG)((wcslen(message) + 1) * sizeof(WCHAR)));
        EVENT_DESCRIPTOR descriptor = { 1, 0, 0, 4, 0, 0, 0 };
        EventWrite(hProvider, &descriptor, 1, eventData);
    }
}

void ProcessXMLAttributes()
{
    IXMLDOMDocument* pXMLDoc = NULL;
    HRESULT hr = CoCreateInstance(__uuidof(DOMDocument60), NULL, CLSCTX_INPROC_SERVER, 
                                  __uuidof(IXMLDOMDocument), (void**)&pXMLDoc);
    
    if (SUCCEEDED(hr))
    {
        VARIANT_BOOL status;
        BSTR bstrXML = SysAllocString(L"<root><item id='1' name='test' value='100' status='active' type='primary'/></root>");
        
        pXMLDoc->loadXML(bstrXML, &status);
        
        IXMLDOMElement* pRoot = NULL;
        pXMLDoc->get_documentElement(&pRoot);
        
        if (pRoot)
        {
            IXMLDOMNodeList* pItems = NULL;
            BSTR bstrTag = SysAllocString(L"item");
            pRoot->getElementsByTagName(bstrTag, &pItems);
            
            if (pItems)
            {
                long length = 0;
                pItems->get_length(&length);
                
                for (long i = 0; i < length; i++)
                {
                    IXMLDOMNode* pItem = NULL;
                    pItems->get_item(i, &pItem);
                    
                    if (pItem)
                    {
                        IXMLDOMNamedNodeMap* pAttrs = NULL;
                        pItem->get_attributes(&pAttrs);
                        
                        if (pAttrs)
                        {
                            long attrCount = 0;
                            pAttrs->get_length(&attrCount);
                            
                            for (long j = 0; j < attrCount; j++)
                            {
                                IXMLDOMNode* pAttr = NULL;
                                pAttrs->get_item(j, &pAttr);
                                
                                if (pAttr)
                                {
                                    VARIANT varValue;
                                    pAttr->get_nodeValue(&varValue);
                                    VariantClear(&varValue);
                                }
                            }
                        }
                        
                        pItem->Release();
                    }
                }
                
                pItems->Release();
            }
            
            SysFreeString(bstrTag);
            pRoot->Release();
        }
        
        SysFreeString(bstrXML);
        pXMLDoc->Release();
    }
}

int main()
{
    CoInitialize(NULL);
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 50000; i++)
    {
        ProcessXMLAttributes();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);
    CoUninitialize();

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

