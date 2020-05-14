#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

int main() {
  int* a = malloc(8); // 2 integers
  int* b = malloc(12); // 3 integers
  a[0] = 1;
  a[1] = 2;
  b[0] = 3;
  b[1] = 4;
  b[2] = 5;
  int* c = malloc(20); // 5 integers
  memcpy(c, a, 8);
  memcpy(c+2, b, 12);
  printf("%d\n", (int)(sizeof(int)));
  printf("%d\n", c[0]); // Excpected 1. Prints 1.
  printf("%d\n", c[1]); // Excpected 2. Prints 2.
  printf("%d\n", c[2]); // Excpected 3. Prints something random.
  printf("%d\n", c[3]); // Excpected 4. Prints something random.
  printf("%d\n", c[4]); // Excpected 5. Prints something random.
  return 0;
}