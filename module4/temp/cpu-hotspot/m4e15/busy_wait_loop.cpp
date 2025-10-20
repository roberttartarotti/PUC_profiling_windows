#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <chrono>

static const GUID ProviderGuid = 
{ 0xA2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE1 } };

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

void ProcessTask(int iterations)
{
    volatile int result = 0;
    auto start = std::chrono::high_resolution_clock::now();
    auto end = start + std::chrono::milliseconds(100);
    
    while (std::chrono::high_resolution_clock::now() < end)
    {
        result++;
    }
    
    for (int i = 0; i < iterations; i++)
    {
        result += i * i;
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 5000; i++)
    {
        ProcessTask(10000);
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

