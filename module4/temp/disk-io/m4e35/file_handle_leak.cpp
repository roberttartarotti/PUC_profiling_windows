#include <windows.h>
#include <stdio.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0xB4345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF2 } };

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

void CreateAndWriteFiles()
{
    wchar_t filename[256];
    
    for (int i = 0; i < 500; i++)
    {
        swprintf_s(filename, 256, L"tempfile_%d.dat", i);
        
        HANDLE hFile = CreateFileW(filename,
                                   GENERIC_WRITE,
                                   0,
                                   NULL,
                                   CREATE_ALWAYS,
                                   FILE_ATTRIBUTE_NORMAL,
                                   NULL);
        
        if (hFile != INVALID_HANDLE_VALUE)
        {
            char buffer[1024];
            DWORD bytesWritten;
            
            for (int j = 0; j < 1024; j++)
            {
                buffer[j] = (char)(j % 256);
            }
            
            for (int k = 0; k < 100; k++)
            {
                WriteFile(hFile, buffer, 1024, &bytesWritten, NULL);
            }
        }
    }
}

void CleanupFiles()
{
    wchar_t filename[256];
    
    for (int i = 0; i < 500; i++)
    {
        swprintf_s(filename, 256, L"tempfile_%d.dat", i);
        DeleteFileW(filename);
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 10; i++)
    {
        CreateAndWriteFiles();
        CleanupFiles();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

