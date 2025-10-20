#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <msxml6.h>
#include <comdef.h>

static const GUID ProviderGuid = 
{ 0xC3345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE9 } };

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

void EvaluateXPath()
{
    IXMLDOMDocument* pXMLDoc = NULL;
    HRESULT hr = CoCreateInstance(__uuidof(DOMDocument60), NULL, CLSCTX_INPROC_SERVER, 
                                  __uuidof(IXMLDOMDocument), (void**)&pXMLDoc);
    
    if (SUCCEEDED(hr))
    {
        VARIANT_BOOL status;
        BSTR bstrXML = SysAllocString(
            L"<catalog>"
            L"<book id='1'><title>Book1</title><author>Author1</author><price>29.99</price></book>"
            L"<book id='2'><title>Book2</title><author>Author2</author><price>39.99</price></book>"
            L"<book id='3'><title>Book3</title><author>Author3</author><price>19.99</price></book>"
            L"<book id='4'><title>Book4</title><author>Author4</author><price>49.99</price></book>"
            L"<book id='5'><title>Book5</title><author>Author5</author><price>24.99</price></book>"
            L"</catalog>"
        );
        
        pXMLDoc->loadXML(bstrXML, &status);
        
        for (int i = 0; i < 5000; i++)
        {
            IXMLDOMNodeList* pNodes = NULL;
            BSTR bstrXPath = SysAllocString(L"//book[price > 25]");
            pXMLDoc->selectNodes(bstrXPath, &pNodes);
            
            if (pNodes)
            {
                long length = 0;
                pNodes->get_length(&length);
                
                for (long j = 0; j < length; j++)
                {
                    IXMLDOMNode* pNode = NULL;
                    pNodes->get_item(j, &pNode);
                    
                    if (pNode)
                    {
                        BSTR text;
                        pNode->get_text(&text);
                        SysFreeString(text);
                        pNode->Release();
                    }
                }
                
                pNodes->Release();
            }
            
            SysFreeString(bstrXPath);
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

    for (int i = 0; i < 100; i++)
    {
        EvaluateXPath();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);
    CoUninitialize();

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

