#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
int NegativeIndex(int i){
if(i<0) goto ERROR;
return 1;
ERROR:;
exit(2);
return 0;
}
int IndexInBounds(int i,int a){
NegativeIndex(i);
if(i>=a) goto ERROR;
return 1;
ERROR:;
exit(3);
return 0;
}
int SizeOfStringArrayInBytes(int s,char* a,int* o){
if(s==0) goto ZER;
return o[s-1]+strlen(a+o[s-1])+1;
ZER:;
return 0;
}
void AssignStringToStringArray(char** a,int i,char* str,int s,int* o){
int l=SizeOfStringArrayInBytes(s,*a,o);
int b=o[i];
int d=l-o[i];
int e=0;
if(i==s-1) goto SKIP;
d=l-o[i+1]+1;
e=l-b-d;
SKIP:;
int f=strlen(str);
char* s1=malloc(b);
memcpy(s1,*a,b);
char* s2=malloc(d);
memcpy(s2,*a+b+e,d);
free(*a);
e=b+d+f;
*a=malloc(e);
memcpy(*a,s1,b);
memcpy(*a+b,str,f);
memcpy(*a+b+f,s2,d);
b=i+1;
d=e-l;
START:;
if(b==s) goto END;
o[b]=o[b]+d;
b=b+1;
goto START;
END:;
}
char* GetElementFromStringArray(char* a,int i,int* o){
return a+o[i];
}
void InitializeStringArray(char* a,int s,int* o){
int i=0;
o[0]=0;
START:;
if(i==s) goto SKIP;
a[i]='\0';
o[i]=i;
i=i+1;
goto START;
SKIP:;
}
void CopyIntegerPointer(int** dest,int* src,int s){
free(*dest);
*dest=malloc(sizeof(int)*s);
memcpy(*dest,src,sizeof(int)*s);
}
void CopyCharPointer(char** dest,char* src,int s){
free(*dest);
*dest=malloc(sizeof(char)*s);
memcpy(*dest,src,sizeof(char)*s);
}
char* MakeStringVar(char* value){
char* s=malloc(strlen(value)+1);
strcpy(s,value);
return s;
}
char* ConcatStrings(char* s1,char* s2){
size_t l1=strlen(s1);
char* r=malloc(l1+strlen(s2)+1);
strcpy(r,s1);
strcpy(r+l1,s2);
return r;
}
char* ConcatStringArrays(char* a1,char* a2,int s1,int s2,int* l1,int* l2){
int tl1=SizeOfStringArrayInBytes(s1,a1,l1);
int tl2=SizeOfStringArrayInBytes(s2,a2,l2);
char* n=malloc(tl1+tl2);
memcpy(n,a1,tl1);
memcpy(n+tl1,a2,tl2);
return n;
}
char* StringArrayToString(char* a, int size,int* l){
if (size==0) goto EMPTY;
int t=SizeOfStringArrayInBytes(size,a,l)+2*size+2;
char* s=malloc(t);
strcpy(s,"[");
t=0;
int o=1;
LOOP:;
char* p=GetElementFromStringArray(a,t,l);
int length=(int)(strlen(p));
strcpy(s+o,"\"");
o=o+1;
memcpy(s+o,p,length);
o=o+length;
strcpy(s+o,"\"");
o=o+1;
if(t==size-1) goto END;
strcpy(s+o,",");
o=o+1;
t=t+1;
goto LOOP;
END:;
strcpy(s+o,"]");
goto FINISH;
EMPTY:;
s=malloc(3);
strcpy(s,"[]");
FINISH:;
return s;
}
int* ConcatIntegerArrays(int* ia1,int* ia2,int s1,int s2){
size_t b1=sizeof(int)*s1;
size_t b2=sizeof(int)*s2;
int* n=malloc(b1+b2);
memcpy(n,ia1,b1);
memcpy(n+s1,ia2,b2);
return n;
}
int* CountNewOffsets(char* a,int* ia1,int* ia2,int s1,int s2){
int o=SizeOfStringArrayInBytes(s1,a,ia1);
int* k=malloc(sizeof(int)*s2);
int c=0;
LOOP:;
if(c==s2) goto END;
k[c]=o+ia2[c];
c=c+1;
goto LOOP;
END:;
int* n=ConcatIntegerArrays(ia1,k,s1,s2);
free(k);
return n;
}
int main() {
int i0=3;
NegativeIndex(i0);
int i1=i0;
char* s0=malloc(sizeof(char)*i1);
int* i2=malloc(sizeof(int)*i1);
InitializeStringArray(s0,i1,i2);
i0=2;
NegativeIndex(i0);
int i3=i0;
char* s1=malloc(sizeof(char)*i3);
int* i4=malloc(sizeof(int)*i3);
InitializeStringArray(s1,i3,i4);
char* s2=MakeStringVar("eka_a");
i0=0;
IndexInBounds(i0,i1);
AssignStringToStringArray(&s0,i0,s2,i1,i2);
free(s2);
s2=MakeStringVar("toka_a");
i0=1;
IndexInBounds(i0,i1);
AssignStringToStringArray(&s0,i0,s2,i1,i2);
free(s2);
s2=MakeStringVar("kolmas_a");
i0=2;
IndexInBounds(i0,i1);
AssignStringToStringArray(&s0,i0,s2,i1,i2);
free(s2);
s2=MakeStringVar("eka_b");
i0=0;
IndexInBounds(i0,i3);
AssignStringToStringArray(&s1,i0,s2,i3,i4);
free(s2);
s2=MakeStringVar("toka_b");
i0=1;
IndexInBounds(i0,i3);
AssignStringToStringArray(&s1,i0,s2,i3,i4);
i0=1;
NegativeIndex(i0);
int i5=i0;
char* s3=malloc(sizeof(char)*i5);
int* i6=malloc(sizeof(int)*i5);
InitializeStringArray(s3,i5,i6);
free(s2);
s2=MakeStringVar("Both arrays before concat: ");
printf("%s\n",s2);
free(s2);
s2=StringArrayToString(s0,i1,i2);
printf("%s\n",s2);
free(s2);
s2=StringArrayToString(s1,i3,i4);
printf("%s\n",s2);
char* s4=ConcatStringArrays(s1,s0,i3,i1,i4,i2);
i0=i3+i1;
int* i7=CountNewOffsets(s4,i4,i2,i3,i1);
/******* Start of assigning array s4 to s3 *********/
int i8=SizeOfStringArrayInBytes(i0,s4,i7);
CopyCharPointer(&s3,s4,i8);
i5=i0;
CopyIntegerPointer(&i6,i7,i0);
/******* End of assigning array s4 to s3 *********/
free(s2);
s2=MakeStringVar("Toimisko tää: ");
char* s5=StringArrayToString(s3,i5,i6);
printf("%s%s\n",s2,s5);
free(s5);
s5=MakeStringVar("Entä tää ");
free(s2);
s2=StringArrayToString(s3,i5,i6);
char* s6=ConcatStrings(s5,s2);
printf("%s\n",s6);
free(s6);
s6=MakeStringVar("UUs arvo c[2]");
i0=2;
IndexInBounds(i0,i5);
AssignStringToStringArray(&s3,i0,s6,i5,i6);
printf("%d\n",i5);
i0=0;
IndexInBounds(i0,i5);
free(s6);
s6=GetElementFromStringArray(s3,i0,i6);
i0=(int)(strlen(s6));
printf("%d\n",i0);
i0=2;
printf("%d\n",i0);
return 0;
}
