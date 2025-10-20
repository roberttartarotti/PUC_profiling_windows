#include <windows.h>
#include <stdio.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0xD4345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF4 } };

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

void CreateDeleteFiles()
{
    wchar_t filename[256];
    
    for (int i = 0; i < 5000; i++)
    {
        swprintf_s(filename, 256, L"temp_%d.tmp", i);
        
        HANDLE hFile = CreateFileW(filename,
                                   GENERIC_WRITE,
                                   0,
                                   NULL,
                                   CREATE_ALWAYS,
                                   FILE_ATTRIBUTE_NORMAL,
                                   NULL);
        
        if (hFile != INVALID_HANDLE_VALUE)
        {
            char data[256];
            DWORD bytesWritten;
            
            for (int j = 0; j < 256; j++)
            {
                data[j] = (char)(j % 256);
            }
            
            WriteFile(hFile, data, 256, &bytesWritten, NULL);
            CloseHandle(hFile);
        }
        
        DeleteFileW(filename);
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 50; i++)
    {
        CreateDeleteFiles();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

