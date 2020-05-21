#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

void NegativeIndex(int i) {
  if (i < 0) goto ERROR;
  goto END;
  ERROR:;
  printf("%s", "Index is smaller than zero");
  END:;
}
void IndexInBounds(int i, int arrSize) {
  NegativeIndex(i);
  if (i >= arrSize) goto ERROR;
  goto END;
  ERROR:;
  printf("%s", "Index is bigger than allowed");
  END:;
}
int CalculateSumOfArray(int size, int* a) {
  int counter = 0;
  int result = 0;
  START:;
  if(counter==size) goto CONT;
  result=result+a[counter];
  counter=counter+1;
  goto START;
  CONT:;
  return result;
}
/*
writer.WriteLine("int SizeOfStringArrayInBytes(int s,char* a,int* o){");
      writer.WriteLine("if(s==0) goto ZER;");
      writer.WriteLine("return s+strlen(a+o[s-1]);");
      writer.WriteLine("ZER:;");
      writer.WriteLine("return 0;");
      writer.WriteLine("}");
*/

/*void GetElementFromStringArray(char** s, char* a, int i, int* l) {
  int o=0;
  if(i==0) goto SKIP;
  int c=0;
  START:;
  if(c==i) goto SKIP;
  o=o+l[c];
  c=c+1;
  goto START;
  SKIP:;
  // *s = realloc(*s, lengths[i]);
  memcpy(*s, a+o, l[i]);
}*/
void InitializeStringArray(char* a,int s,int* l){
  int i=0;
  START:;
  if(i==s) goto SKIP;
  a[i]='\0';
  l[i]=1;
  i=i+1;
  goto START;
  SKIP:;
}
// CopyIntegerPointer({dest.Id},{src.Id},{src.Size.Id})

void CopyCharPointer(char** dest,char* src,int s){
  *dest=realloc(*dest,s);
  memcpy(*dest,src,s);
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
void IntegerToStringWithSizeCalc(int i,char** s) {
  int t=IntegerSizeAsString(i);
  *s=realloc(*s,t+1);
  sprintf(*s,"%d",i);
}
void IntegerToString(int i,int size,char** s) {
  *s=realloc(*s,size+1);
  sprintf(*s,"%d",i);
}
char* ConcatStrings(char* s1,char* s2){
  size_t l1=strlen(s1);
  char* r=malloc(l1+strlen(s2)+1);
  strcpy(r,s1);
  strcpy(r+l1,s2);
  return r;
}
int* ConcatIntegerArrays(int* ia1, int* ia2, int s1, int s2){
  size_t b1=sizeof(int)*s1;
  size_t b2=sizeof(int)*s2;
  int* n=malloc(b1+b2);
  memcpy(n,ia1,b1);
  memcpy(n+s1,ia2,b2);
  return n;
}
char* IntegerArrayToString(int* a,int size){
  // Handle size == 0 first
  if(size==0) goto EMPTY;
  // Calculate the size of the result array first (No need to realloc all the time)
  int* lengths=malloc(sizeof(int)*size);
  int totalSize=0;
  int c=0;
  int temp=0;
  START:;
  if(c==size) goto CONT;
  temp=IntegerSizeAsString(a[c]);
  lengths[c]=temp;
  totalSize=totalSize+temp;
  c=c+1;
  goto START;
  CONT:;

  // Count the amount of commas
  c=size-1;

  // Realloc the destination string once
  char* s=malloc(totalSize+c+3);
  strcpy(s, "[");

  // Next convert the integers into strings and copy them to *s
  c=0;
  temp=1; // Offset
  LOOP:;
  sprintf(s+temp,"%d",a[c]);
  temp=temp+lengths[c];
  if(c==size-1) goto END;
  strcpy(s+temp,",");
  temp=temp+1;
  c=c+1;
  goto LOOP;
  END:;
  strcpy(s+temp,"]");
  goto FINISH;
  EMPTY:;
  s=malloc(3);
  strcpy(s,"[]");
  FINISH:;
  return s;
  /*char* v=malloc(2);
  strcpy(v,"[");
  char* comma=malloc(2);
  strcpy(comma,",");
  int counter=-1;
  START:;
  counter=counter+1;
  if(counter==size) goto END;
  int element=a[counter];
  char* elementString=malloc(1);
  IntegerToString(element,&elementString);
  char* temp=malloc(1);
  ConcatStrings(&temp,v,elementString);
  v=realloc(v,strlen(temp)+2);
  sprintf(v,"%s",temp);
  if(counter==size-1) goto START;
  char* fin=malloc(1);
  ConcatStrings(&fin,temp,comma);
  v=realloc(v,strlen(fin)+1);
  sprintf(v,"%s",fin);
  goto START;
  END:;
  sprintf(v+strlen(v),"%s","]");
  *s=realloc(*s,strlen(v)+1);
  sprintf(*s,"%s",v);*/
}
/*
CodeGeneration.Variable v = InitializeTempVariable(BuiltInType.String, "3"); // [ and null and ]
 
// Initialize a string variable for [
CodeGeneration.Variable comma = InitializeTempVariable(BuiltInType.String, "2");
CopyStringVariable(comma.Id, "\",\"");
this.writer.Write($"sprintf({v.Id},\"%s\",\"[\");\n");

CodeGeneration.Variable counter = InitializeTempVariable(BuiltInType.Integer);
string startLoop = this.labelGenerator.GenerateLabel();
string endLoop = this.labelGenerator.GenerateLabel();
// ALL THESE VARIABLES NEED TO REMAIN THE SAME (Do not free them at any point of loop)
this.writer.Write($"{counter.Id}=-1;\n");
this.writer.Write($"{startLoop}:;\n");
this.writer.Write($"{counter.Id}={counter.Id}+1;\n"); // Starts at 0
this.writer.Write($"if({counter.Id}=={ia.Size.Id}) goto {endLoop};\n");
CodeGeneration.Variable element = InitializeTempVariable(BuiltInType.Integer);
this.writer.Write($"{element.Id}={ia.Id}[{counter.Id}];\n");
CodeGeneration.Variable elementString = ConvertIntegerToString(element);
HandleArrayAddition(v, elementString); // Both are freed
CodeGeneration.Variable temp = this.stack.Pop();

// Copy the element string to v
// CopyMemory($"{v.Id}+{SizeOf(v)}", elementString.Id, SizeOf(elementString));
// Copy the comma to v
ReallocMemory(v.Id, $"{SizeOf(temp)}+2"); // Add space for ] too
this.writer.Write($"sprintf({v.Id},\"%s\",{temp.Id});\n");
this.writer.Write($"if({counter.Id}=={ia.Size.Id}-1) goto {startLoop};\n"); // Last element
// this.writer.Write($"sprintf({v.Id}+{SizeOf(v)},\"%s\",\",\");\n");
HandleArrayAddition(temp, comma);
CodeGeneration.Variable fin = this.stack.Pop();
// Assign this fin variables value to the Id of v
ReallocMemory(v.Id, $"{SizeOf(fin)}+1"); // NO need to add space for ] here
this.writer.Write($"sprintf({v.Id},\"%s\",{fin.Id});\n");
this.writer.Write($"goto {startLoop};\n");

this.writer.Write($"{endLoop}:;\n");
this.writer.Write($"sprintf({v.Id}+{SizeOf(v)},\"%s\",\"]\");\n");
*/
char* MakeStringVar(char* value){
  printf("strlen(): %d\n", (int)(strlen(value)));
  char* s=malloc(strlen(value)+1);
  strcpy(s,value);
  return s;
}
int SizeOfStringArrayInBytes(int s,char* a,int* o){
  // ['\0','j','o','o','\0','\0']
  // [0,1,5]
  if(s==0) goto ZER;
  return o[s-1]+strlen(a+o[s-1])+1;
  ZER:;
  return 0;
}
void CopyIntegerPointer(int** dest,int* src,int s){
  // *dest=realloc(*src,s);
  free(*dest);
  *dest=malloc(sizeof(int)*s);
  memcpy(*dest,src,sizeof(int)*s);
}
char* ConcatStringArrays(char* a1,char* a2,int s1,int s2,int* l1,int* l2){
  int tl1=SizeOfStringArrayInBytes(s1,a1,l1);
  int tl2=SizeOfStringArrayInBytes(s2,a2,l2);
  char* n=malloc(tl1+tl2);
  memcpy(n,a1,tl1);
  memcpy(n+tl1,a2,tl2);
  return n;
}
/*
void CopyIntegerPointer(int** dest,int* src,int s){
  *dest=realloc(*dest,s);
  memcpy(*dest,src,s);
}
*/


/*
writer.WriteLine("char* GetElementFromStringArray(char* a,int i,int* l){");
      // writer.WriteLine("char* s=malloc(l[i]);");
      writer.WriteLine("int o=0;");
      writer.WriteLine("if(i==0) goto SKIP;");
      writer.WriteLine("int c=0;");
      writer.WriteLine("START:;");
      writer.WriteLine("if(c==i) goto SKIP;");
      writer.WriteLine("o=o+l[c];");
      writer.WriteLine("c=c+1;");
      writer.WriteLine("goto START;");
      writer.WriteLine("SKIP:;");

      // The new line
      writer.WriteLine("char* s=a+o;");
      // writer.WriteLine("memcpy(s,a+o,l[i]);");
      writer.WriteLine("return s;");
      writer.WriteLine("}");
*/
char* GetElementFromStringArray(char* a,int i,int* o){
  return a+o[i];
}
/*
writer.WriteLine("int SizeOfStringArrayInBytes(int s,char* a,int* o){");
writer.WriteLine("if(s==0) goto ZER;");
writer.WriteLine("return s+strlen(a+o[size-1]);");
writer.WriteLine("ZER:;");
writer.WriteLine("return 0;");
*/
void ChangeArrayElement(int* a){
  a[1]=87;
}
void AssignStringToStringArray(char** a, int i, char* str, int s, int* o) {
  // NEW STRING DOES NOT COPY '\0' !!!!!
  // ['\0','\0','\0']. offsets [0,1,2]
  // Assign "joo" to index 2
  // ['\0','\0','j','o','o','\0']
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

  // Also change the offsets starting from i+1
  b=i+1;
  d=e-l;
  START:;
  if(b==s) goto END;
  o[b]=o[b]+d;
  b=b+1;
  goto START;
  END:;
}
char* StringArrayToString(char* a, int size,int* l){
//char* StringArrayToString(){
  char* a=malloc(10);
  a[0]='e';
  a[1]='k';
  a[2]='a';
  a[3]='\0';
  a[4]='t';
  a[5]='o';
  a[6]='k';
  a[7]='a';
  a[8]='\0';
  a[9]='\0';
  int size=3;
  int* l=malloc(10);
  l[0]=0;
  l[1]=4;
  l[2]=9;
  // ['e','k','a','\0','t','o','k','a','\0','\0'] third is empty string
  if (size==0) goto EMPTY;
  int t=SizeOfStringArrayInBytes(size,a,l)+2*size+2;
  char* s=malloc(t);
  strcpy(s, "[");

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

  // Surrounders are same as in IntegerArrayToString
  EMPTY:;
  s=malloc(3);
  strcpy(s,"[]");
  FINISH:;
  printf("%s\n", s);
  return s;
}


int main() {
  StringArrayToString();
  // AssignStringToStringArray(char** a, int i, char* str, int s, int* o)
  char* taulukko=malloc(3);
  taulukko[0]='\0';
  taulukko[1]='\0';
  taulukko[2]='\0';
  int* offarit=malloc(sizeof(int)*3);
  offarit[0]=0;
  offarit[1]=1;
  offarit[2]=2;
  char* lisaa=malloc(4);
  strcpy(lisaa, "joo");

  AssignStringToStringArray(&taulukko,2,lisaa,3,offarit);
  free(lisaa);
  lisaa=malloc(6);
  strcpy(lisaa, "Terve");
  AssignStringToStringArray(&taulukko,0,lisaa,3,offarit);

  free(lisaa);
  lisaa=malloc(5);
  strcpy(lisaa, "kusi");
  AssignStringToStringArray(&taulukko,1,lisaa,3,offarit);

  free(lisaa);
  lisaa=malloc(5);
  strcpy(lisaa, "kusi");
  AssignStringToStringArray(&taulukko,0,lisaa,3,offarit);
  int koko=SizeOfStringArrayInBytes(3,taulukko,offarit);
  // int koko = 6;
  printf("KOKO: %d\n", koko);
  for (int k=0; k<koko; k++){
    printf("taulukko[%d]: %c\n", k, taulukko[k]);
  }


  /*int* esim=malloc(sizeof(int)*3);
  esim[0]=1;
  esim[1]=1;
  esim[2]=1;
  ChangeArrayElement(esim);
  printf("esim[1]= %d\n", esim[1]);
  StringArrayToString();
  char* kusi=MakeStringVar("jepp");
  free(kusi);
  kusi=MakeStringVar("Uus");
  int* intti=malloc(sizeof(int)*4);
  intti[0]=1;
  intti[1]=2;
  intti[2]=3;
  intti[3]=4;
  int* uusi=malloc(sizeof(int)*1);
  // uusi=realloc(intti,sizeof(int)*4);
  CopyIntegerPointer(&uusi,intti,4);
  printf("uusi[0]: %d\n",uusi[0]);
  printf("uusi[1]: %d\n",uusi[1]);
  printf("uusi[2]: %d\n",uusi[2]);
  printf("uusi[3]: %d\n",uusi[3]);
  printf("intti[0]: %d\n",intti[0]);
  printf("intti[1]: %d\n",intti[1]);
  printf("intti[2]: %d\n",intti[2]);
  printf("intti[3]: %d\n",intti[3]);

  int* kakka = ConcatIntegerArrays(uusi,intti,4,4);
  for (int p = 0; p < 8; p++) printf("kakka[%d]: %d\n",p,kakka[p]);
  // char* paska=malloc(1);
  char* paska = IntegerArrayToString(intti,0);
  char* haha = ConcatStrings(kusi,paska);
  printf("Concateated string: %s\n", haha);
  printf("Array as string: %s\n", paska);
  paska[2]='R';
  printf("Array as string: %s\n", paska);
  paska=realloc(paska, 4);
  IndexInBounds(-3, 4);
  int size = 4;
  int* a = malloc(size*sizeof(int));
  a[0] = 1;
  a[1] = 2;
  a[2] = 3;
  a[3] = 4;
  int res = CalculateSumOfArray(size, a);
  printf("result: %d\n", res);
  char* str = malloc(5);
  strcpy(str, "moro");
  char* stringArr = malloc(3);
  stringArr[0] = '\0';
  stringArr[1] = '\0';
  stringArr[2] = '\0';
  int* offsets=malloc(sizeof(int)*3);
  offsets[0]=0;
  offsets[1]=1;
  offsets[2]=2;
  int strArrSize=SizeOfStringArrayInBytes(3,stringArr,offsets);
  printf(" THE SIZE: %d\n", strArrSize);
  int* lengths = malloc(sizeof(int)*3);
  lengths[0] = 1;
  lengths[1] = 1;
  lengths[2] = 1;
  // AssignStringToStringArray(&stringArr, 0, str, 3, lengths);
  printf("%s", "The new string array: ");
  printf("%c", stringArr[0]);
  printf("%c", stringArr[1]);
  printf("%c", stringArr[2]);
  printf("%c", stringArr[3]);
  printf("%c", stringArr[4]);
  printf("%c", stringArr[5]);
  printf("%c", stringArr[6]);
  printf("%c", stringArr[7]);
  printf("%c", stringArr[8]);
  printf("%c", stringArr[9]);
  printf("%c", stringArr[10]);
  printf("%c", stringArr[11]);
  printf("%c", stringArr[12]);
  printf("%c", stringArr[13]);
  printf("%c", stringArr[14]);
  printf("%c\n", stringArr[15]);
  AssignStringToStringArray(&stringArr, 1, str, 3, lengths);
  printf("%s", "The new string array: ");
  printf("%c", stringArr[0]);
  printf("%c", stringArr[1]);
  printf("%c", stringArr[2]);
  printf("%c", stringArr[3]);
  printf("%c", stringArr[4]);
  printf("%c", stringArr[5]);
  printf("%c", stringArr[6]);
  printf("%c", stringArr[7]);
  printf("%c", stringArr[8]);
  printf("%c", stringArr[9]);
  printf("%c", stringArr[10]);
  printf("%c", stringArr[11]);
  printf("%c", stringArr[12]);
  printf("%c", stringArr[13]);
  printf("%c", stringArr[14]);
  printf("%c\n", stringArr[15]);
  AssignStringToStringArray(&stringArr, 2, str, 3, lengths);
  printf("%s", "The new string array: ");
  printf("%c", stringArr[0]);
  printf("%c", stringArr[1]);
  printf("%c", stringArr[2]);
  printf("%c", stringArr[3]);
  printf("%c", stringArr[4]);
  printf("%c", stringArr[5]);
  printf("%c", stringArr[6]);
  printf("%c", stringArr[7]);
  printf("%c", stringArr[8]);
  printf("%c", stringArr[9]);
  printf("%c", stringArr[10]);
  printf("%c", stringArr[11]);
  printf("%c", stringArr[12]);
  printf("%c", stringArr[13]);
  printf("%c", stringArr[14]);
  printf("%c\n", stringArr[15]);
  AssignStringToStringArray(&stringArr, 1, "joo", 3, lengths);
  printf("%s", "The new string array: ");
  printf("%c", stringArr[0]);
  printf("%c", stringArr[1]);
  printf("%c", stringArr[2]);
  printf("%c", stringArr[3]);
  printf("%c", stringArr[4]);
  printf("%c", stringArr[5]);
  printf("%c", stringArr[6]);
  printf("%c", stringArr[7]);
  printf("%c", stringArr[8]);
  printf("%c", stringArr[9]);
  printf("%c", stringArr[10]);
  printf("%c", stringArr[11]);
  printf("%c", stringArr[12]);
  printf("%c", stringArr[13]);
  printf("%c", stringArr[14]);
  printf("%c\n", stringArr[15]);
  // GetElementFromStringArray(char** s, char** a, int i, int* lengths)
  char* element = GetElementFromStringArray(stringArr,0,lengths);
  // GetElementFromStringArray(&element, stringArr, 1, lengths);
  printf("Element: %s\n", element);

  char* newArr = malloc(4);
  int* len = malloc(sizeof(int)*4);
  InitializeStringArray(newArr, 4, len);
  printf("Initialized: %d\n", len[0]);
  printf("Initialized: %d\n", len[1]);
  printf("Initialized: %d\n", len[2]);
  printf("Initialized: %d\n", len[3]);*/


  /*// The declaration. Add the StringLengths (lengths) variable to StringArray variable
  // Use SetSize method for it, it will make the lengths var IsArraySize
  char* a = malloc(3); // Init with 3 null characters
  int* lengths = malloc(3*sizeof(int));
  int i = 0;
  L100:;
  if (i==3) goto L89;
  a[i] = '\0';
  lengths[i] = 1;
  i = i + 1;
  goto L100;

  L89:;*/
  /********* Adding a string to array **********/
  /*int index = 1;
  char* b = malloc(6);
  strcpy(b, "Terve");

  // Count the total length of the original lengths
  int counter = 0;
  int temp = 0;
  S1:;
  if (counter == 3) goto S9;
  temp = temp + lengths[counter];
  counter = counter + 1;
  goto S1;

  S9:;

  // TODO: Need to copy the original lengths to a new temprary array
  int* originalLengths = malloc(sizeof(int) * temp); // Need to know total length before this
  memcpy(originalLengths, lengths, sizeof(int) * temp);

  // Change the lenght of the string in original arrays index
  lengths[index]=(int)(strlen(b)+1);

  // Count the total length of new lengths
  counter = 0;
  temp = 0;
  BEGIN:;
  if (counter == 3) goto JOO;
  temp = temp + lengths[counter];
  counter = counter + 1;
  goto BEGIN;

  JOO:;
  printf("totalLength is %d\n", temp);
  char* c = malloc(temp);

  counter = 0;
  // int offset = 0;
  int originalOffsets = 0;
  temp = 0;
  START:;
  if (counter == 3) goto END;
  char* stringToCopy = a+originalOffsets;
  if(counter!=index) goto CONT;
  stringToCopy = b;
  CONT:;
  memcpy(c+temp, stringToCopy, lengths[counter]);
  counter = counter + 1;
  temp = temp + lengths[counter-1];
  originalOffsets = originalOffsets + originalLengths[counter-1];
  goto START;

  END:; // Empty statements*/

  // The new array is stored in c

  /*************** ******************/

  /*********** Get the element in index *************/
  /*int o = 0;
  if (index==0) goto SKIP;
  int cnt = 0;
  L1:
  if (cnt==index+1) goto SKIP;
  o = o + lengths[cnt];
  cnt = cnt+1;
  goto L1;
  SKIP:;
  a = malloc(lengths[index]);
  memcpy(a, c+o, lengths[index]);
  printf("element in index %d: %s\n", index, a);*/
  /***************************************************/
  return 0;
}