#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <msxml6.h>
#include <comdef.h>

static const GUID ProviderGuid = 
{ 0xE3345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xEB } };

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

void SerializeToXML()
{
    IXMLDOMDocument* pXMLDoc = NULL;
    HRESULT hr = CoCreateInstance(__uuidof(DOMDocument60), NULL, CLSCTX_INPROC_SERVER, 
                                  __uuidof(IXMLDOMDocument), (void**)&pXMLDoc);
    
    if (SUCCEEDED(hr))
    {
        IXMLDOMElement* pRoot = NULL;
        BSTR bstrRoot = SysAllocString(L"data");
        pXMLDoc->createElement(bstrRoot, &pRoot);
        
        if (pRoot)
        {
            pXMLDoc->putref_documentElement(pRoot);
            
            for (int i = 0; i < 200; i++)
            {
                IXMLDOMElement* pItem = NULL;
                BSTR bstrItem = SysAllocString(L"item");
                pXMLDoc->createElement(bstrItem, &pItem);
                
                if (pItem)
                {
                    wchar_t idValue[32];
                    swprintf_s(idValue, 32, L"%d", i);
                    BSTR bstrId = SysAllocString(L"id");
                    VARIANT varId;
                    varId.vt = VT_BSTR;
                    varId.bstrVal = SysAllocString(idValue);
                    pItem->setAttribute(bstrId, varId);
                    
                    wchar_t textValue[256];
                    swprintf_s(textValue, 256, L"Content for item %d with some additional data to make it larger", i);
                    BSTR bstrText = SysAllocString(textValue);
                    pItem->put_text(bstrText);
                    
                    pRoot->appendChild(pItem, NULL);
                    
                    SysFreeString(bstrId);
                    SysFreeString(bstrText);
                    VariantClear(&varId);
                    pItem->Release();
                }
                
                SysFreeString(bstrItem);
            }
            
            for (int i = 0; i < 50; i++)
            {
                BSTR xmlString = NULL;
                pXMLDoc->get_xml(&xmlString);
                
                volatile size_t len = SysStringLen(xmlString);
            }
            
            pRoot->Release();
        }
        
        SysFreeString(bstrRoot);
        pXMLDoc->Release();
    }
}

int main()
{
    CoInitialize(NULL);
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 500; i++)
    {
        SerializeToXML();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);
    CoUninitialize();

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

