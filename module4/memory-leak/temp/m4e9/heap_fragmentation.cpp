#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <vector>

static const GUID ProviderGuid = 
{ 0x82345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF7 } };

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

void FragmentHeap()
{
    std::vector<void*> pointers;
    
    for (int i = 0; i < 100; i++)
    {
        int size = (i % 10 + 1) * 100;
        void* ptr = malloc(size);
        pointers.push_back(ptr);
    }
    
    for (int i = 1; i < 100; i += 2)
    {
        free(pointers[i]);
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 300; i++)
    {
        FragmentHeap();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

