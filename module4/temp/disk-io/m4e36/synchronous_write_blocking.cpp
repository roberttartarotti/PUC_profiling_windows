#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <vector>

static const GUID ProviderGuid = 
{ 0xC4345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF3 } };

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

void WriteLargeFile(int threadId)
{
    wchar_t filename[256];
    swprintf_s(filename, 256, L"largefile_%d.dat", threadId);
    
    HANDLE hFile = CreateFileW(filename,
                               GENERIC_WRITE,
                               0,
                               NULL,
                               CREATE_ALWAYS,
                               FILE_FLAG_WRITE_THROUGH,
                               NULL);
    
    if (hFile != INVALID_HANDLE_VALUE)
    {
        char* buffer = new char[65536];
        DWORD bytesWritten;
        
        for (int i = 0; i < 65536; i++)
        {
            buffer[i] = (char)(i % 256);
        }
        
        for (int i = 0; i < 200; i++)
        {
            WriteFile(hFile, buffer, 65536, &bytesWritten, NULL);
        }
        
        delete[] buffer;
        CloseHandle(hFile);
    }
    
    DeleteFileW(filename);
}

void RunThreadedWrites()
{
    std::vector<std::thread> threads;
    
    for (int i = 0; i < 8; i++)
    {
        threads.emplace_back(WriteLargeFile, i);
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

    for (int i = 0; i < 10; i++)
    {
        RunThreadedWrites();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

