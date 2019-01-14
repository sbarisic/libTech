#include <stdint.h>
#include <stdio.h>
#include <Windows.h>

// nVidia
_declspec(dllexport) unsigned long NvOptimusEnablement = 0x00000001;

// AMD
_declspec(dllexport) DWORD AmdPowerXpressRequestHighPerformance = 0x00000001;

typedef int(__cdecl *EntryFunc)(void);

int main(int argc, const char** argv) {
	HANDLE nvapi64 = LoadLibrary("nvapi64.dll");
	HANDLE libTechExe = LoadLibrary("libTech_Bootstrapper.dll");

	if (!libTechExe) {
		printf("Could not find libTech_Bootstrapper.dll");
		return 1;
	}

	EntryFunc Entry = (EntryFunc)GetProcAddress(libTechExe, "Entry");
	if (!Entry) {
		printf("Could not find entry point 'Entry' in libTech_Bootstrapper.dll");
		return 2;
	}

	return	Entry();
}