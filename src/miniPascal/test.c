#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
char* MakeStringVar(char* value){
char* s=malloc(strlen(value)+1);
strcpy(s,value);
return s;
}
int hello(){
char* s0=MakeStringVar("Hello");
printf("%s\n",s0);
return 0;
}
int printhello(int *i0,int i1){
hello();
int b0=*i0==i1;
if(!b0) goto L0;
int i2=0;
return i2;
goto L1;
L0:;
i2=1;
int i3=*i0+i2;
*i0=i3;
i3=printhello(&*i0,i1);
return i3;
L1:;
}
int main() {
int i0;
int i1=1;
i0=i1;
i1=10;
int i2=printhello(&i0,i1);
return 0;
}
