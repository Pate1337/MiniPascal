#include <stdio.h>
#include <string.h>
#include <math.h>
int main() {
int i0;
int i1;
int i2;
int i3=3;
i0=i3;
i3=5;
int* i4=malloc(i3*sizeof(i3));
i3=4;
int *i5=&i4[i0];
*i5=i3;
i3=4;
int* i6=malloc(i3*sizeof(i3));
i3=2;
int i7=1;
i5=&i6[i7];
*i5=i3;
i3=1;
i5=&i6[i3];
int *i8=&i4[i0];
*i8=*i5;
i8=&i4[i0];
printf("%d", *i8);
printf("\n");
i3=1;
i8=&i6[i3];
printf("%d", *i8);
printf("\n");
i3=4;
i7=1;
i8=&i6[i7];
*i8=i3;
i3=3;
i8=&i4[i3];
printf("%d", *i8);
printf("\n");
i3=1;
i8=&i6[i3];
printf("%d", *i8);
printf("\n");
return 0;
}
