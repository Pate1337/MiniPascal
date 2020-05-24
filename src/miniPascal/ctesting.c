#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

// Array is ref..
void p(int* i0,int *i1){
  free(i0);
  i0=malloc(sizeof(int)*4);
  i0[1]=4;
  printf("In p, a[1] = %d\n", i0[1]);
  *i1 = 5;
}
void refArray(int** i0){
  // Uses *i0 always
  *(*i0+1)=1;
  //int* i1 = malloc(sizeof(int)*2);
  //i1[1]=200;
  //free(*i0);
  //*i0=malloc(sizeof(int)*2);
  //memcpy(*i0, i1, sizeof(int)*2);
}
void refVar(int* i0){
  // Uses *i0 always
  *i0=5;
}
void normalVar(int i0){
  // Uses i0
  i0=1;
}
void normalArr(int* i0){
  // need to malloc new array and copy memory
  // Then set i1 original Id to 
  int* i1=malloc(sizeof(int)*2);
  memcpy(i1, i0, sizeof(int)*2);
  i1[1] = 6;
}
void normalArrayElement(int i0){
  i0 = 89;
}
void refArrayElement(int* i0){
  // Uses *i0 always
  *i0 = 89;
}

// Array with ref
void p2(int* *i0,int *i1){

  *i0[1]=4;
  printf("In p, a[1] = %d\n", *i0[1]);
}

int main() {
  int s = 2;
  int* a=malloc(sizeof(int)*s);
  *(a+1) = 0;
  printf("Before:\n");
  printf("%d\n", *(a+1));
  printf("%d\n", s);
  refArray(&a);
  refVar(&s);
  normalVar(s);
  normalArr(a);
  normalArrayElement(*(a+1));
  refArrayElement(&a[1]);
  printf("After:\n");
  printf("%d\n", *(a+1));
  printf("%d\n", s);
  return 0;
}