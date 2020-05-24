namespace CodeGeneration
{
  public class FunctionCreator
  {
    public bool NegativeIndex { get; set; }
    public bool IndexInBounds { get; set; }
    public bool AssignStringToStringArray { get; set; }
    public bool SizeOfStringArrayInBytes { get; set; } // Maybe not necessary
    public bool GetElementFromStringArray { get; set; }
    public bool StringArrayInitialization { get; set; }
    public bool CopyIntegerPointer { get; set; }
    public bool CopyCharPointer { get; set; }
    public bool IntegerSizeAsString { get; set; }
    public bool IntegerToStringWithSizeCalc { get; set; }
    public bool IntegerArrayToString { get; set; }
    public bool MakeStringVar { get; set; }
    public bool ConcatStrings { get; set; }
    public bool ConcatIntegerArrays { get; set; }
    public bool ConcatStringArrays { get; set; }
    public bool StringArrayToString { get; set; }
    public bool CountNewOffsets { get; set; }
    public bool CompareStrings { get; set; }

    public FunctionCreator()
    {
      this.NegativeIndex = false;
      this.IndexInBounds = false;
      this.AssignStringToStringArray = false;
      this.SizeOfStringArrayInBytes = false;
      this.GetElementFromStringArray = false;
      this.StringArrayInitialization = false;
      this.CopyIntegerPointer = false;
      this.CopyCharPointer = false;
      this.IntegerSizeAsString = false;
      this.IntegerToStringWithSizeCalc = false;
      this.IntegerArrayToString = false;
      this.MakeStringVar = false;
      this.ConcatStrings = false;
      this.ConcatIntegerArrays = false;
      this.ConcatStringArrays = false;
      this.StringArrayToString = false;
      this.CountNewOffsets = false;
      this.CompareStrings = false;
    }

    public void WriteFunctions(FileHandler.FileWriter writer)
    {
      if (this.NegativeIndex) WriteNegativeIndexFunction(writer);
      if (this.IndexInBounds) WriteIndexInBoundsFunction(writer);
      if (this.SizeOfStringArrayInBytes) WriteSizeOfStringArrayInBytesFunction(writer);
      if (this.AssignStringToStringArray) WriteAssignStringToStringArrayFunction(writer);
      if (this.GetElementFromStringArray) WriteGetElementFromStringArrayFunction(writer);
      if (this.StringArrayInitialization) WriteStringArrayInitializationFunction(writer);
      if (this.CopyIntegerPointer) WriteCopyIntegerPointerFunction(writer);
      if (this.CopyCharPointer) WriteCopyCharPointerFunction(writer);
      if (this.IntegerSizeAsString) WriteIntegerSizeAsStringFunction(writer);
      if (this.IntegerToStringWithSizeCalc) WriteIntegerToStringWithSizeCalcFunction(writer);
      if (this.IntegerArrayToString) WriteIntegerArrayToStringFunction(writer);
      if (this.MakeStringVar) WriteMakeStringVarFunction(writer);
      if (this.ConcatStrings) WriteConcatStringsFunction(writer);
      if (this.ConcatIntegerArrays) WriteConcatIntegerArraysFunction(writer);
      if (this.ConcatStringArrays) WriteConcatStringArraysFunction(writer);
      if (this.StringArrayToString) WriteStringArrayToStringFunction(writer);
      if (this.CountNewOffsets) WriteCountNewOffsetsFunction(writer);
      if (this.CompareStrings) WriteCompareStringsFunction(writer);
    }
    private void WriteNegativeIndexFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("int NegativeIndex(int i){");
      writer.WriteLine("if(i<0) goto ERROR;");
      writer.WriteLine("return 1;");
      writer.WriteLine("ERROR:;");
      writer.WriteLine($"exit({(int)ErrorCode.NegativeIndex});");
      writer.WriteLine("return 0;");
      writer.WriteLine("}");
      this.NegativeIndex = true;
    }
    private void WriteIndexInBoundsFunction(FileHandler.FileWriter writer)
    {
      if (!this.NegativeIndex) WriteNegativeIndexFunction(writer);
      writer.WriteLine("int IndexInBounds(int i,int a){");
      writer.WriteLine("NegativeIndex(i);");
      writer.WriteLine("if(i>=a) goto ERROR;");
      writer.WriteLine("return 1;");
      writer.WriteLine("ERROR:;");
      writer.WriteLine($"exit({(int)ErrorCode.OutOfBoundsIndex});");
      writer.WriteLine("return 0;");
      writer.WriteLine("}");
      this.IndexInBounds = true;
    }
    private void WriteSizeOfStringArrayInBytesFunction(FileHandler.FileWriter writer)
    {
      // First offset is always 0.
      // For example the offsets of [ '\0', '\0', '\0' ] are  [0,1,2]
      // ['m','o','i','\0','v','a','a','n','\0','\0'] = [0,4,9]
      writer.WriteLine("int SizeOfStringArrayInBytes(int s,char* a,int* o){");
      writer.WriteLine("if(s==0) goto ZER;");
      writer.WriteLine("return o[s-1]+strlen(a+o[s-1])+1;");
      writer.WriteLine("ZER:;");
      writer.WriteLine("return 0;");
      writer.WriteLine("}");
      this.SizeOfStringArrayInBytes = true;
    }
    private void WriteAssignStringToStringArrayFunction(FileHandler.FileWriter writer)
    {
      if (!this.SizeOfStringArrayInBytes) WriteSizeOfStringArrayInBytesFunction(writer);
      writer.WriteLine("void AssignStringToStringArray(char** a,int i,char* str,int s,int* o){");
      writer.WriteLine("int l=SizeOfStringArrayInBytes(s,*a,o);");
      writer.WriteLine("int b=o[i];");
      writer.WriteLine("int d=1;");
      writer.WriteLine("int e=0;");
      writer.WriteLine("if(i==s-1) goto SKIP;");
      writer.WriteLine("d=l-o[i+1]+1;");
      writer.WriteLine("e=l-b-d;");
      writer.WriteLine("SKIP:;");
      writer.WriteLine("int f=strlen(str);");
      writer.WriteLine("char* s1=malloc(b);");
      writer.WriteLine("memcpy(s1,*a,b);");
      writer.WriteLine("char* s2=malloc(d);");
      writer.WriteLine("memcpy(s2,*a+b+e,d);");
      writer.WriteLine("free(*a);");
      writer.WriteLine("e=b+d+f;");
      writer.WriteLine("*a=malloc(e);");
      writer.WriteLine("memcpy(*a,s1,b);");
      writer.WriteLine("memcpy(*a+b,str,f);");
      writer.WriteLine("memcpy(*a+b+f,s2,d);");
      writer.WriteLine("*(*a+b+f+d-1)='\\0';");
      writer.WriteLine("b=i+1;");
      writer.WriteLine("d=e-l;");
      writer.WriteLine("START:;");
      writer.WriteLine("if(b==s) goto END;");
      writer.WriteLine("o[b]=o[b]+d;");
      writer.WriteLine("b=b+1;");
      writer.WriteLine("goto START;");
      writer.WriteLine("END:;");
      writer.WriteLine("}");
      this.AssignStringToStringArray = true;
    }
    private void WriteGetElementFromStringArrayFunction(FileHandler.FileWriter writer)
    {
      // TODO: Don't even make a function of this
      writer.WriteLine("char* GetElementFromStringArray(char* a,int i,int* o){");
      writer.WriteLine("return a+o[i];");
      writer.WriteLine("}");
      this.GetElementFromStringArray = true;
    }
    private void WriteStringArrayInitializationFunction(FileHandler.FileWriter writer)
    {
      // o must be alloc'd with malloc(sizeof(int)) if size == 0 
      writer.WriteLine("void InitializeStringArray(char* a,int s,int* o){");
      writer.WriteLine("int i=0;");
      writer.WriteLine("o[0]=0;"); // Always have at least this
      writer.WriteLine("START:;");
      writer.WriteLine("if(i==s) goto SKIP;");
      writer.WriteLine("a[i]='\\0';");
      writer.WriteLine("o[i]=i;");
      writer.WriteLine("i=i+1;");
      writer.WriteLine("goto START;");
      writer.WriteLine("SKIP:;");
      writer.WriteLine("}");
      this.StringArrayInitialization = true;
    }
    private void WriteCopyIntegerPointerFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("void CopyIntegerPointer(int** dest,int* src,int s){");
      writer.WriteLine("free(*dest);");
      writer.WriteLine("*dest=malloc(sizeof(int)*s);");
      writer.WriteLine("memcpy(*dest,src,sizeof(int)*s);");
      writer.WriteLine("}");
      this.CopyIntegerPointer = true;
    }
    private void WriteCopyCharPointerFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("void CopyCharPointer(char** dest,char* src,int s){");
      writer.WriteLine("free(*dest);");
      writer.WriteLine("*dest=malloc(sizeof(char)*s);");
      writer.WriteLine("memcpy(*dest,src,sizeof(char)*s);");
      writer.WriteLine("}");
      this.CopyCharPointer = true;
    }
    private void WriteIntegerSizeAsStringFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("int IntegerSizeAsString(int i){");
      writer.WriteLine("int t=1;");
      writer.WriteLine("if(i<0) goto NEG;");
      writer.WriteLine("if(i==0) goto END;");
      writer.WriteLine("if(i==1) goto END;");
      writer.WriteLine("t=(int)(ceil(log10(i)));");
      writer.WriteLine("goto END;");
      writer.WriteLine("NEG:;");
      writer.WriteLine("t=-1*i;");
      writer.WriteLine("t=(int)(ceil(log10(t))+1);");
      writer.WriteLine("END:;");
      writer.WriteLine("return t;");
      writer.WriteLine("}");
      this.IntegerSizeAsString = true;
    }
    private void WriteIntegerToStringWithSizeCalcFunction(FileHandler.FileWriter writer)
    {
      if (!this.IntegerSizeAsString) WriteIntegerSizeAsStringFunction(writer);
      writer.WriteLine("void IntegerToStringWithSizeCalc(int i,char** s){");
      writer.WriteLine("int t=IntegerSizeAsString(i);");
      writer.WriteLine("*s=realloc(*s,t+1);");
      writer.WriteLine("sprintf(*s,\"%d\",i);");
      writer.WriteLine("}");
      this.IntegerToStringWithSizeCalc = true;
    }
    private void WriteIntegerArrayToStringFunction(FileHandler.FileWriter writer)
    {
      if (!this.IntegerSizeAsString) WriteIntegerSizeAsStringFunction(writer);
      writer.WriteLine("char* IntegerArrayToString(int* a,int size){");
      writer.WriteLine("if(size==0) goto EMPTY;");
      writer.WriteLine("int* l=malloc(sizeof(int)*size);");
      writer.WriteLine("int ts=0;");
      writer.WriteLine("int c=0;");
      writer.WriteLine("int t=0;");
      writer.WriteLine("START:;");
      writer.WriteLine("if(c==size) goto CONT;");
      writer.WriteLine("t=IntegerSizeAsString(a[c]);");
      writer.WriteLine("l[c]=t;");
      writer.WriteLine("ts=ts+t;");
      writer.WriteLine("c=c+1;");
      writer.WriteLine("goto START;");
      writer.WriteLine("CONT:;");
      writer.WriteLine("c=size-1;");
      writer.WriteLine("char* s=malloc(ts+c+3);");
      writer.WriteLine("strcpy(s,\"[\");");
      writer.WriteLine("c=0;");
      writer.WriteLine("t=1;");
      writer.WriteLine("LOOP:;");
      writer.WriteLine("sprintf(s+t,\"%d\",a[c]);");
      writer.WriteLine("t=t+l[c];");
      writer.WriteLine("if(c==size-1) goto END;");
      writer.WriteLine("strcpy(s+t,\",\");");
      writer.WriteLine("t=t+1;");
      writer.WriteLine("c=c+1;");
      writer.WriteLine("goto LOOP;");
      writer.WriteLine("END:;");
      writer.WriteLine("strcpy(s+t,\"]\");");
      writer.WriteLine("goto FINISH;");
      writer.WriteLine("EMPTY:;");
      writer.WriteLine("s=malloc(3);");
      writer.WriteLine("strcpy(s,\"[]\");");
      writer.WriteLine("FINISH:;");
      writer.WriteLine("return s;");
      writer.WriteLine("}");
      this.IntegerArrayToString = true;
    }
    private void WriteMakeStringVarFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("char* MakeStringVar(char* value){");
      writer.WriteLine("char* s=malloc(strlen(value)+1);");
      writer.WriteLine("strcpy(s,value);");
      writer.WriteLine("return s;");
      writer.WriteLine("}");
      this.MakeStringVar = true;
    }
    private void WriteConcatStringsFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("char* ConcatStrings(char* s1,char* s2){");
      writer.WriteLine("size_t l1=strlen(s1);");
      writer.WriteLine("char* r=malloc(l1+strlen(s2)+1);");
      writer.WriteLine("strcpy(r,s1);");
      writer.WriteLine("strcpy(r+l1,s2);");
      writer.WriteLine("return r;");
      writer.WriteLine("}");
      this.ConcatStrings = true;
    }
    private void WriteConcatIntegerArraysFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("int* ConcatIntegerArrays(int* ia1,int* ia2,int s1,int s2){");
      writer.WriteLine("size_t b1=sizeof(int)*s1;");
      writer.WriteLine("size_t b2=sizeof(int)*s2;");
      writer.WriteLine("int* n=malloc(b1+b2);");
      writer.WriteLine("memcpy(n,ia1,b1);");
      writer.WriteLine("memcpy(n+s1,ia2,b2);");
      writer.WriteLine("return n;");
      writer.WriteLine("}");
      this.ConcatIntegerArrays = true;
    }
    private void WriteCountNewOffsetsFunction(FileHandler.FileWriter writer)
    {
      if (!this.SizeOfStringArrayInBytes) WriteSizeOfStringArrayInBytesFunction(writer);
      if (!this.ConcatIntegerArrays) WriteConcatIntegerArraysFunction(writer);
      writer.WriteLine("int* CountNewOffsets(char* a,int* ia1,int* ia2,int s1,int s2){");
      writer.WriteLine("int o=SizeOfStringArrayInBytes(s1,a,ia1);");
      writer.WriteLine("int* k=malloc(sizeof(int)*s2);");
      writer.WriteLine("int c=0;");
      writer.WriteLine("LOOP:;");
      writer.WriteLine("if(c==s2) goto END;");
      writer.WriteLine("k[c]=o+ia2[c];");
      writer.WriteLine("c=c+1;");
      writer.WriteLine("goto LOOP;");
      writer.WriteLine("END:;");
      writer.WriteLine("int* n=ConcatIntegerArrays(ia1,k,s1,s2);");
      writer.WriteLine("free(k);");
      writer.WriteLine("return n;");
      writer.WriteLine("}");
      this.CountNewOffsets = true;
    }
    private void WriteConcatStringArraysFunction(FileHandler.FileWriter writer)
    {
      if (!this.SizeOfStringArrayInBytes) WriteSizeOfStringArrayInBytesFunction(writer);
      writer.WriteLine("char* ConcatStringArrays(char* a1,char* a2,int s1,int s2,int* l1,int* l2){");
      writer.WriteLine("int tl1=SizeOfStringArrayInBytes(s1,a1,l1);");
      writer.WriteLine("int tl2=SizeOfStringArrayInBytes(s2,a2,l2);");
      writer.WriteLine("char* n=malloc(tl1+tl2);");
      writer.WriteLine("memcpy(n,a1,tl1);");
      writer.WriteLine("memcpy(n+tl1,a2,tl2);");
      writer.WriteLine("return n;");
      writer.WriteLine("}");
      this.ConcatStringArrays = true;
    }
    private void WriteStringArrayToStringFunction(FileHandler.FileWriter writer)
    {
      if (!this.SizeOfStringArrayInBytes) WriteSizeOfStringArrayInBytesFunction(writer);
      if (!this.GetElementFromStringArray) WriteGetElementFromStringArrayFunction(writer);
      writer.WriteLine("char* StringArrayToString(char* a, int size,int* l){");
      writer.WriteLine("if (size==0) goto EMPTY;");
      writer.WriteLine("int t=SizeOfStringArrayInBytes(size,a,l)+2*size+2;");
      writer.WriteLine("char* s=malloc(t);");
      writer.WriteLine("strcpy(s,\"[\");");
      writer.WriteLine("t=0;");
      writer.WriteLine("int o=1;");
      writer.WriteLine("LOOP:;");
      writer.WriteLine("char* p=GetElementFromStringArray(a,t,l);");
      writer.WriteLine("int length=(int)(strlen(p));");
      writer.WriteLine("strcpy(s+o,\"\\\"\");");
      writer.WriteLine("o=o+1;");
      writer.WriteLine("memcpy(s+o,p,length);");
      writer.WriteLine("o=o+length;");
      writer.WriteLine("strcpy(s+o,\"\\\"\");");
      writer.WriteLine("o=o+1;");
      writer.WriteLine("if(t==size-1) goto END;");
      writer.WriteLine("strcpy(s+o,\",\");");
      writer.WriteLine("o=o+1;");
      writer.WriteLine("t=t+1;");
      writer.WriteLine("goto LOOP;");
      writer.WriteLine("END:;");
      writer.WriteLine("strcpy(s+o,\"]\");");
      writer.WriteLine("goto FINISH;");
      writer.WriteLine("EMPTY:;");
      writer.WriteLine("s=malloc(3);");
      writer.WriteLine("strcpy(s,\"[]\");");
      writer.WriteLine("FINISH:;");
      writer.WriteLine("return s;");
      writer.WriteLine("}");
      this.StringArrayToString = true;
    }
    private void WriteCompareStringsFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("int CompareStrings(char* s1,char* s2){");
      writer.WriteLine("int l=(int)(strlen(s1));");
      writer.WriteLine("if(l!=(int)(strlen(s2))) return 0;");
      writer.WriteLine("int c=0;");
      writer.WriteLine("LOOP:;");
      writer.WriteLine("if(c==l) return 1;"); // Made it to end

      writer.WriteLine("if(s1[c]!=s2[c]) return 0;");
      writer.WriteLine("c=c+1;");
      writer.WriteLine("goto LOOP;");

      writer.WriteLine("}");
      this.CompareStrings = true;
    }
  }
}