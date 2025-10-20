#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <string>
#include <sstream>

static const GUID ProviderGuid = 
{ 0xD2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE4 } };

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

std::string* BuildLargeXML(int items)
{
    std::string* xml = new std::string("<root>");
    
    for (int i = 0; i < items; i++)
    {
        *xml += "<item id='";
        *xml += std::to_string(i);
        *xml += "'>";
        *xml += "<name>Item";
        *xml += std::to_string(i);
        *xml += "</name>";
        *xml += "<description>This is a description for item ";
        *xml += std::to_string(i);
        *xml += "</description>";
        *xml += "<value>";
        *xml += std::to_string(i * 100);
        *xml += "</value>";
        *xml += "</item>";
    }
    
    *xml += "</root>";
    return xml;
}

void ProcessXMLGeneration()
{
    for (int i = 0; i < 100; i++)
    {
        std::string* result = BuildLargeXML(500);
        volatile size_t len = result->length();
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 200; i++)
    {
        ProcessXMLGeneration();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

