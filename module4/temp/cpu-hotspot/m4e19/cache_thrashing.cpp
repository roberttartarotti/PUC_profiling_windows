#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <vector>

static const GUID ProviderGuid = 
{ 0xE2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE5 } };

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

struct alignas(64) CacheLineData
{
    volatile long counter;
    char padding[60];
};

CacheLineData globalCounters[8];

void IncrementCounter(int threadId)
{
    for (int i = 0; i < 10000000; i++)
    {
        InterlockedIncrement(&globalCounters[threadId % 8].counter);
    }
}

void RunThreads()
{
    std::vector<std::thread> threads;
    
    for (int i = 0; i < 8; i++)
    {
        threads.emplace_back(IncrementCounter, i);
    }
    
    for (auto& t : threads)
    {
        t.join();
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 8; i++)
    {
        globalCounters[i].counter = 0;
    }

    RunThreads();

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

