#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0x62345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF5 } };

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

void GrowBuffer()
{
    char* buffer = (char*)malloc(100);
    
    strcpy_s(buffer, 100, "Initial data");
    
    buffer = (char*)realloc(buffer, 1000);
    
    buffer = (char*)malloc(2000);
    
    free(buffer);
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 800; i++)
    {
        GrowBuffer();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

