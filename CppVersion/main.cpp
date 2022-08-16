#include <iostream>
#include "uisave_fmt.h"

int main(int argc, char **argv)
{
    std::string file_name;
    if (argc >= 2)
    {
        file_name = argv[1];
    }
    else
    {
        std::cout << "input UISAVE.DAT location:" << std::endl;
        std::cin >> file_name;
    }

    UisaveFmt uisave_fmt(file_name);
    if (!uisave_fmt.Init())
    {
        return 0;
    }

    uisave_fmt.Parse();
    return 0;
}
