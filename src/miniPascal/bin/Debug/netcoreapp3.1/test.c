#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
int main() {
int i0=4;
if(i0<0) goto ERROR;
int i1=i0;
char* =malloc(sizeof());
int* i2=malloc(sizeof(int)*i1);
int i3=0;
L0:;
if(i3==i1) goto L1;
[i3]='\0';
i2[i3]=1;
i3=i3+1;
goto L0;
L1:;
goto END;
ERROR:
printf("Error occurred!\n");
END:
return 0;
}
