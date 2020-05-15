#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

int main() {
  // The declaration. Add the StringLengths (lengths) variable to StringArray variable
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

  L89:;
  /********* Adding a string to array **********/
  int index = 1;
  char* b = malloc(6);
  strcpy(b, "Terve");
  // Change the lenght of the string in original arrays index
  lengths[index]=(int)(strlen(b)+1);

  // Count the total length
  int counter = 0;
  int temp = 0;
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
  temp = 0;
  START:;
  if (counter == 3) goto END;
  char* stringToCopy = a;
  if(counter!=index) goto CONT;
  stringToCopy = b;
  CONT:;
  memcpy(c+temp, stringToCopy, lengths[counter]);
  counter = counter + 1;
  temp = temp + lengths[counter-1];
  goto START;

  END:; // Empty statements

  // The new array is stored in c

  /*************** ******************/

  /*********** Get the element in index *************/
  int o = 0;
  if (index==0) goto SKIP;
  int cnt = 0;
  L1:
  if (cnt==index) goto SKIP;
  o = o + lengths[index-1];
  cnt = cnt+1;
  goto L1;
  SKIP:;
  a = malloc(lengths[index]);
  memcpy(a, c+o, lengths[index]);
  printf("element in index %d: %s\n", index, a);
  /***************************************************/
  return 0;
}