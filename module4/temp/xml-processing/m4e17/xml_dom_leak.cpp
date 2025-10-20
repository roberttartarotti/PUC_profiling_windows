#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <msxml6.h>
#include <comdef.h>

static const GUID ProviderGuid = 
{ 0xC2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE3 } };

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

void ParseXMLDocument()
{
    IXMLDOMDocument* pXMLDoc = NULL;
    HRESULT hr = CoCreateInstance(__uuidof(DOMDocument60), NULL, CLSCTX_INPROC_SERVER, 
                                  __uuidof(IXMLDOMDocument), (void**)&pXMLDoc);
    
    if (SUCCEEDED(hr))
    {
        VARIANT_BOOL status;
        BSTR bstrXML = SysAllocString(L"<root><item id='1'><data>Value1</data></item><item id='2'><data>Value2</data></item><item id='3'><data>Value3</data></item></root>");
        
        pXMLDoc->loadXML(bstrXML, &status);
        
        IXMLDOMNodeList* pNodeList = NULL;
        BSTR bstrQuery = SysAllocString(L"//item");
        pXMLDoc->selectNodes(bstrQuery, &pNodeList);
        
        if (pNodeList)
        {
            long length = 0;
            pNodeList->get_length(&length);
            
            for (long i = 0; i < length; i++)
            {
                IXMLDOMNode* pNode = NULL;
                pNodeList->get_item(i, &pNode);
                
                BSTR nodeText;
                if (pNode)
                {
                    pNode->get_text(&nodeText);
                    SysFreeString(nodeText);
                }
            }
        }
        
        SysFreeString(bstrXML);
        SysFreeString(bstrQuery);
    }
}

int main()
{
    CoInitialize(NULL);
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 5000; i++)
    {
        ParseXMLDocument();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);
    CoUninitialize();

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

