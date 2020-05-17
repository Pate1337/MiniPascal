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
void AssignStringToStringArray(char** a, int i, char* str, int s, int* l) {
  int totalLengthOfOriginal = CalculateSumOfArray(s, l);

  // Need to copy lengths before changing
  int* originalLengths = malloc(sizeof(int)*s);
  memcpy(originalLengths, l, sizeof(int)*s);

  int t = (int)(strlen(str)+1);
  l[i] = t;

  int difference = t - originalLengths[i];
  int newLength = totalLengthOfOriginal + difference;
  // int newLength = CalculateSumOfArray(size, lengths);

  // Create new string array
  char* n = malloc(newLength);

  // Copy the new array to c
  int c = 0;
  int temp = 0; // temp
  int originalOffset = 0; // originalOffset
  START:;
  if (c==s) goto END;
  char* sc = malloc(l[c]);
  memcpy(sc, *a+originalOffset, originalLengths[c]);
  if (c!=i) goto CONT;
  memcpy(sc, str, l[c]);
  CONT:;
  memcpy(n+temp, sc, l[c]);
  c=c+1;
  temp=temp+l[c-1];
  originalOffset=originalOffset+originalLengths[c-1];
  goto START;
  END:;
  // At the end, realloc a and assign c to it
  *a = realloc(*a, newLength);
  memcpy(*a, n, newLength);
}
void GetElementFromStringArray(char** s, char* a, int i, int* l) {
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
}
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
void CopyIntegerPointer(int** dest,int* src,int s){
  *dest=realloc(*dest,s);
  memcpy(*dest,src,s);
}
void CopyCharPointer(char** dest,char* src,int s){
  *dest=realloc(*dest,s);
  memcpy(*dest,src,s);
}

int main() {
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
  int* lengths = malloc(sizeof(int)*3);
  lengths[0] = 1;
  lengths[1] = 1;
  lengths[2] = 1;
  AssignStringToStringArray(&stringArr, 0, str, 3, lengths);
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
  char* element = malloc(lengths[1]);
  GetElementFromStringArray(&element, stringArr, 1, lengths);
  printf("Element: %s\n", element);

  char* newArr = malloc(4);
  int* len = malloc(sizeof(int)*4);
  InitializeStringArray(newArr, 4, len);
  printf("Initialized: %d\n", len[0]);
  printf("Initialized: %d\n", len[1]);
  printf("Initialized: %d\n", len[2]);
  printf("Initialized: %d\n", len[3]);
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