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
int d=1;
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
*(*a+b+f+d-1)='\0';
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
int IntegerSizeAsString(int i){
int t=1;
if(i<0) goto NEG;
if(i==0) goto END;
if(i==1) goto END;
t=(int)(ceil(log10(i)));
goto END;
NEG:;
t=-1*i;
t=(int)(ceil(log10(t))+1);
END:;
return t;
}
char* IntegerArrayToString(int* a,int size){
if(size==0) goto EMPTY;
int* l=malloc(sizeof(int)*size);
int ts=0;
int c=0;
int t=0;
START:;
if(c==size) goto CONT;
t=IntegerSizeAsString(a[c]);
l[c]=t;
ts=ts+t;
c=c+1;
goto START;
CONT:;
c=size-1;
char* s=malloc(ts+c+3);
strcpy(s,"[");
c=0;
t=1;
LOOP:;
sprintf(s+t,"%d",a[c]);
t=t+l[c];
if(c==size-1) goto END;
strcpy(s+t,",");
t=t+1;
c=c+1;
goto LOOP;
END:;
strcpy(s+t,"]");
goto FINISH;
EMPTY:;
s=malloc(3);
strcpy(s,"[]");
FINISH:;
return s;
}
char* MakeStringVar(char* value){
char* s=malloc(strlen(value)+1);
strcpy(s,value);
return s;
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
int p(char* *s0,int *i0,int* *i1){
char* s1=MakeStringVar("size of array: ");
printf("%s%d\n",s1,*i0);
int i2=3;
int b0=*i0==i2;
if(!b0) goto L1;
return 0;
L1:;
free(s1);
s1=MakeStringVar("TERSE");
i2=0;
IndexInBounds(i2,*i0);
AssignStringToStringArray(&*s0,i2,s1,*i0,*i1);
return 0;
}
int p2(char* *s0,int *i0,int* *i1){
char* s1=MakeStringVar("TERSE");
int i2=1;
IndexInBounds(i2,*i0);
AssignStringToStringArray(&*s0,i2,s1,*i0,*i1);
return 0;
}
int* createintegerarray(int *i0){
int i1=2;
NegativeIndex(i1);
int i2=i1;
int* i3=malloc(sizeof(int)*i2);
i1=1;
int i4=0;
IndexInBounds(i4,i2);
*(i3+i4)=i1;
i1=2;
i4=1;
IndexInBounds(i4,i2);
*(i3+i4)=i1;
*i0=i2;
return i3;
}
char* createstringarray(int *i0,int* *i1){
int i2=2;
NegativeIndex(i2);
int i3=i2;
char* s0=malloc(sizeof(char)*i3);
int* i4=malloc(sizeof(int)*i3);
InitializeStringArray(s0,i3,i4);
char* s1=MakeStringVar("eka");
i2=0;
IndexInBounds(i2,i3);
AssignStringToStringArray(&s0,i2,s1,i3,i4);
free(s1);
s1=MakeStringVar("toka");
i2=1;
IndexInBounds(i2,i3);
AssignStringToStringArray(&s0,i2,s1,i3,i4);
*i0=i3;
CopyIntegerPointer(&*i1,i4,i3);
return s0;
}
int main() {
int i0=2;
NegativeIndex(i0);
int i1=i0;
int* i2=malloc(sizeof(int)*i1);
i0=0;
int* i3=createintegerarray(&i0);
CopyIntegerPointer(&i2,i3,i0);
i1=i0;
char* s0=IntegerArrayToString(i2,i1);
printf("%s\n",s0);
i0=3;
NegativeIndex(i0);
int i4=i0;
char* s1=malloc(sizeof(char)*i4);
free(i3);
i3=malloc(sizeof(int)*i4);
InitializeStringArray(s1,i4,i3);
i0=0;
int* i5=malloc(sizeof(int));
char* s2=createstringarray(&i0,&i5);
int i6=SizeOfStringArrayInBytes(i0,s2,i5);
CopyCharPointer(&s1,s2,i6);
i4=i0;
CopyIntegerPointer(&i3,i5,i0);
free(s0);
s0=StringArrayToString(s1,i4,i3);
printf("%s\n",s0);
i0=3;
NegativeIndex(i0);
i6=i0;
s2=realloc(s2,sizeof(char)*i6);
free(i5);
i5=malloc(sizeof(int)*i6);
InitializeStringArray(s2,i6,i5);
free(s0);
s0=MakeStringVar("init1");
i0=0;
IndexInBounds(i0,i6);
AssignStringToStringArray(&s2,i0,s0,i6,i5);
free(s0);
s0=MakeStringVar("init2");
i0=1;
IndexInBounds(i0,i6);
AssignStringToStringArray(&s2,i0,s0,i6,i5);
free(s0);
s0=MakeStringVar("init3");
i0=2;
IndexInBounds(i0,i6);
AssignStringToStringArray(&s2,i0,s0,i6,i5);
free(s0);
s0=MakeStringVar("a before call: ");
char* s3=StringArrayToString(s2,i6,i5);
printf("%s%s\n",s0,s3);
p(&s2,&i6,&i5);
free(s3);
s3=MakeStringVar("a after call: ");
free(s0);
s0=StringArrayToString(s2,i6,i5);
printf("%s%s\n",s3,s0);
p2(&s2,&i6,&i5);
free(s0);
s0=MakeStringVar("a after p2 call: ");
free(s3);
s3=StringArrayToString(s2,i6,i5);
printf("%s%s\n",s0,s3);
return 0;
}
