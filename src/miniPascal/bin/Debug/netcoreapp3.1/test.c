#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
int NegativeIndex(int i){
if(i<0) goto ERROR;
return 1;
ERROR:;
printf("%s\n", "Index is smaller than zero");
return 0;
}
int IndexInBounds(int i,int a){
if(!NegativeIndex(i)) return 0;
if(i>=a) goto ERROR;
return 1;
ERROR:;
printf("%s\n", "Index is bigger than allowed");
return 0;
}
int CalculateSumOfArray(int s,int* a){
int c = 0;
int r = 0;
START:;
if(c==s) goto CONT;
r=r+a[c];
c=c+1;
goto START;
CONT:;
return r;
}
void AssignStringToStringArray(char** a,int i,char* str,int s,int* l){
int o=CalculateSumOfArray(s,l);
int* ol=malloc(sizeof(int)*s);
memcpy(ol,l,sizeof(int)*s);
int t=(int)(strlen(str)+1);
l[i]=t;
t=t-ol[i];
t=o+t;
char* n=malloc(t);
int c=0;
int of=0;
o=0;
START:;
if(c==s) goto END;
char* sc=malloc(l[c]);
memcpy(sc,*a+of,ol[c]);
if(c!=i) goto CONT;
memcpy(sc,str,l[c]);
CONT:;
memcpy(n+o,sc,l[c]);
c=c+1;
o=o+l[c-1];
of=of+ol[c-1];
goto START;
END:;
*a=realloc(*a,t);
memcpy(*a,n,t);
}
void GetElementFromStringArray(char** s,char* a,int i,int* l){
int o=0;
if(i==0) goto SKIP;
int c=0;
START:;
if(c==i) goto SKIP;
o=o+l[c];
c=c+1;
goto START;
SKIP:;
memcpy(*s,a+o,l[i]);
}
void InitializeStringArray(char* a,int s,int* l){
int i=0;
START:;
if(i==s) goto SKIP;
a[i]=' ';
l[i]=1;
i=i+1;
goto START;
SKIP:;
}
void CopyIntegerPointer(int** dest,int* src,int s){
*dest=realloc(*dest,sizeof(int)*s);
memcpy(*dest,src,sizeof(int)*s);
}
void CopyCharPointer(char** dest,char* src,int s){
*dest=realloc(*dest,s);
memcpy(*dest,src,s);
}
int main() {
int i0=4;
if(!NegativeIndex(i0)) goto ERROR;
int i1=i0;
char* s0=malloc(sizeof(char)*i1);
int* i2=malloc(sizeof(int)*i1);
InitializeStringArray(s0,i1,i2);
char* s1=malloc(10);
strcpy(s1,"Moro vaan");
i0=0;
if(!IndexInBounds(i0,i1)) goto ERROR;
AssignStringToStringArray(&s0,i0,s1,i1,i2);
s1=realloc(s1,5);
strcpy(s1,"Toka");
i0=1;
if(!IndexInBounds(i0,i1)) goto ERROR;
AssignStringToStringArray(&s0,i0,s1,i1,i2);
s1=realloc(s1,7);
strcpy(s1,"Kolmas");
i0=2;
if(!IndexInBounds(i0,i1)) goto ERROR;
AssignStringToStringArray(&s0,i0,s1,i1,i2);
s1=realloc(s1,5);
strcpy(s1,"Vika");
i0=3;
if(!IndexInBounds(i0,i1)) goto ERROR;
AssignStringToStringArray(&s0,i0,s1,i1,i2);
i0=0;
if(!NegativeIndex(i0)) goto ERROR;
int i3=i0;
char* s2=malloc(sizeof(char)*i3);
int* i4=malloc(sizeof(int)*i3);
InitializeStringArray(s2,i3,i4);
/******* Start of assigning array s0 to s2 *********/
i0=CalculateSumOfArray(i1,i2);
CopyCharPointer(&s2,s0,i0);
i3=i1;
CopyIntegerPointer(&i4,i2,i1);
/******* End of assigning array s0 to s2 *********/
s1=realloc(s1,14);
strcpy(s1,"Uuutta paskaa");
i0=2;
if(!IndexInBounds(i0,i1)) goto ERROR;
AssignStringToStringArray(&s0,i0,s1,i1,i2);
i0=i1+i3;
printf("%d\n",i0);
i0=0;
if(!IndexInBounds(i0,i1)) goto ERROR;
s1=realloc(s1,i2[i0]);
GetElementFromStringArray(&s1,s0,i0,i2);
printf("%s\n",s1);
i0=1;
if(!IndexInBounds(i0,i1)) goto ERROR;
s1=realloc(s1,i2[i0]);
GetElementFromStringArray(&s1,s0,i0,i2);
printf("%s\n",s1);
i0=2;
if(!IndexInBounds(i0,i1)) goto ERROR;
s1=realloc(s1,i2[i0]);
GetElementFromStringArray(&s1,s0,i0,i2);
printf("%s\n",s1);
i0=3;
if(!IndexInBounds(i0,i1)) goto ERROR;
s1=realloc(s1,i2[i0]);
GetElementFromStringArray(&s1,s0,i0,i2);
printf("%s\n",s1);
i0=0;
if(!IndexInBounds(i0,i3)) goto ERROR;
s1=realloc(s1,i4[i0]);
GetElementFromStringArray(&s1,s2,i0,i4);
printf("%s\n",s1);
i0=1;
if(!IndexInBounds(i0,i3)) goto ERROR;
s1=realloc(s1,i4[i0]);
GetElementFromStringArray(&s1,s2,i0,i4);
printf("%s\n",s1);
i0=2;
if(!IndexInBounds(i0,i3)) goto ERROR;
s1=realloc(s1,i4[i0]);
GetElementFromStringArray(&s1,s2,i0,i4);
printf("%s\n",s1);
i0=3;
if(!IndexInBounds(i0,i3)) goto ERROR;
s1=realloc(s1,i4[i0]);
GetElementFromStringArray(&s1,s2,i0,i4);
printf("%s\n",s1);
i0=10;
if(!NegativeIndex(i0)) goto ERROR;
int i5=i0;
int* i6=malloc(sizeof(int)*i5);
i0=1;
int i7=0;
if(!IndexInBounds(i7,i5)) goto ERROR;
i6[i7]=i0;
i0=2;
i7=1;
if(!IndexInBounds(i7,i5)) goto ERROR;
i6[i7]=i0;
i0=0;
i7=2;
if(!IndexInBounds(i7,i5)) goto ERROR;
i6[i7]=i0;
i0=25;
i7=5;
if(!IndexInBounds(i7,i5)) goto ERROR;
i6[i7]=i0;
i0=9;
i7=7;
if(!IndexInBounds(i7,i5)) goto ERROR;
i6[i7]=i0;
i0=0;
if(!IndexInBounds(i0,i5)) goto ERROR;
i0=i6[i0];
i7=1;
if(!IndexInBounds(i7,i5)) goto ERROR;
i7=i6[i7];
i0=i0+i7;
i7=2;
if(!IndexInBounds(i7,i5)) goto ERROR;
i6[i7]=i0;
i0=2;
i7=3;
i0=i0+i7;
if(!IndexInBounds(i0,i5)) goto ERROR;
i0=i6[i0];
s1=realloc(s1,9);
strcpy(s1,"a[2+3]: ");
printf("%s%d\n",s1,i0);
i0=1;
if(!IndexInBounds(i0,i5)) goto ERROR;
i0=i6[i0];
s1=realloc(s1,7);
strcpy(s1,"a[1]: ");
printf("%s%d\n",s1,i0);
i0=2;
if(!IndexInBounds(i0,i5)) goto ERROR;
i0=i6[i0];
s1=realloc(s1,7);
strcpy(s1,"a[2]: ");
printf("%s%d\n",s1,i0);
goto END;
ERROR:;
printf("Error occurred! Stopped execution.\n");
END:;
return 0;
}
