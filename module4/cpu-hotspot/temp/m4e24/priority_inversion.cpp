#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>

static const GUID ProviderGuid = 
{ 0xD3345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xEA } };

REGHANDLE hProvider = 0;
CRITICAL_SECTION cs;
volatile bool continueRunning = true;

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

void LowPriorityTask()
{
    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_LOWEST);
    
    for (int i = 0; i < 50; i++)
    {
        EnterCriticalSection(&cs);
        
        volatile int work = 0;
        for (int j = 0; j < 10000000; j++)
        {
            work += j;
        }
        
        LeaveCriticalSection(&cs);
        Sleep(10);
    }
}

void HighPriorityTask()
{
    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_HIGHEST);
    
    Sleep(50);
    
    for (int i = 0; i < 100; i++)
    {
        EnterCriticalSection(&cs);
        
        volatile int work = 0;
        for (int j = 0; j < 100; j++)
        {
            work += j;
        }
        
        LeaveCriticalSection(&cs);
    }
}

void MediumPriorityTask()
{
    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_NORMAL);
    
    Sleep(20);
    
    volatile int work = 0;
    for (int i = 0; i < 1000000000; i++)
    {
        work += i;
        if (!continueRunning) break;
    }
}

int main()
{
    InitializeCriticalSection(&cs);
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    std::thread lowThread(LowPriorityTask);
    std::thread medThread(MediumPriorityTask);
    std::thread highThread(HighPriorityTask);
    
    lowThread.join();
    highThread.join();
    continueRunning = false;
    medThread.join();

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);
    DeleteCriticalSection(&cs);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

