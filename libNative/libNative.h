#pragma once

#define AS_STRING(x) AS_STRING2(x)
#define AS_STRING2(x) #x
#define LINE_STRING AS_STRING(__LINE__)

#define EXPORT __declspec(dllexport)
#define C_EXPORT extern "C" EXPORT

//void libNative_assert(bool Cond, const char* file, const char* line);