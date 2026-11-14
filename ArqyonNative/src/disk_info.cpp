#define ARQYON_EXPORTS
#include "../include/arqyon_native.h"

#include <windows.h>

ARQYON_API long get_disk_total_gb(const char* drive) {
    ULARGE_INTEGER total, free, freeForUser;
    if (GetDiskFreeSpaceExA(drive, &freeForUser, &total, &free)) {
        return (long)(total.QuadPart / (1024LL * 1024 * 1024));
    }
    return -1;
}

ARQYON_API long get_disk_free_gb(const char* drive) {
    ULARGE_INTEGER total, free, freeForUser;
    if (GetDiskFreeSpaceExA(drive, &freeForUser, &total, &free)) {
        return (long)(free.QuadPart / (1024LL * 1024 * 1024));
    }
    return -1;
}
