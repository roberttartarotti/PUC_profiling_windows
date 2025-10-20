#include <windows.h>
#include <stdio.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0xA4345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF1 } };

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

void WriteSmallChunks()
{
    HANDLE hFile = CreateFileW(L"output_data.bin",
                               GENERIC_WRITE,
                               0,
                               NULL,
                               CREATE_ALWAYS,
                               FILE_ATTRIBUTE_NORMAL,
                               NULL);
    
    if (hFile != INVALID_HANDLE_VALUE)
    {
        char buffer[16];
        DWORD bytesWritten;
        
        for (int i = 0; i < 50000; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                buffer[j] = (char)(i % 256);
            }
            
            WriteFile(hFile, buffer, 16, &bytesWritten, NULL);
            FlushFileBuffers(hFile);
        }
        
        CloseHandle(hFile);
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 20; i++)
    {
        WriteSmallChunks();
        DeleteFileW(L"output_data.bin");
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

