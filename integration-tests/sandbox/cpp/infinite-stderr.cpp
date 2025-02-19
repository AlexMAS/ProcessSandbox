#include <iostream>

using namespace std;

int main()
{
    cout << "STDOUT: Lorem ipsum dolor sit amet, consectetur adipiscing elit." << endl;
    cerr << "STDERR: ";

    while (true)
    {
        cerr << "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam arcu nulla, congue eget lobortis ac, ullamcorper vitae erat. Mauris rhoncus volutpat massa pellentesque faucibus. Nam quis dictum tellus. Aliquam sed dui ac nibh porttitor vehicula vel tincidunt leo. Ut interdum finibus urna, eu commodo orci pretium eget. Ut pellentesque in diam sit amet mattis. Vivamus id orci volutpat libero vestibulum suscipit." << endl;
    }

    return 0;
}
