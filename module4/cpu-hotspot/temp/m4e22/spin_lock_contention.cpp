#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <vector>
#include <atomic>

static const GUID ProviderGuid = 
{ 0xB3345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xE8 } };

REGHANDLE hProvider = 0;
std::atomic<bool> spinLock(false);

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

volatile long sharedCounter = 0;

void AcquireSpinLock()
{
    bool expected = false;
    while (!spinLock.compare_exchange_weak(expected, true))
    {
        expected = false;
    }
}

void ReleaseSpinLock()
{
    spinLock.store(false);
}

void WorkerFunction(int iterations)
{
    for (int i = 0; i < iterations; i++)
    {
        AcquireSpinLock();
        
        volatile int temp = 0;
        for (int j = 0; j < 1000; j++)
        {
            temp += j;
        }
        sharedCounter++;
        
        ReleaseSpinLock();
    }
}

void RunContentionTest()
{
    std::vector<std::thread> threads;
    
    for (int i = 0; i < 16; i++)
    {
        threads.emplace_back(WorkerFunction, 1000);
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

    for (int i = 0; i < 20; i++)
    {
        sharedCounter = 0;
        RunContentionTest();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

