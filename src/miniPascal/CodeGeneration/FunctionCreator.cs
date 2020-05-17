namespace CodeGeneration
{
  public class FunctionCreator
  {
    public bool NegativeIndex { get; set; }
    public bool IndexInBounds { get; set; }
    public bool AssignStringToStringArray { get; set; }
    public bool CalculateSumOfArray { get; set; } // Maybe not necessary
    public bool GetElementFromStringArray { get; set; }
    public bool StringArrayInitialization { get; set; }
    public bool CopyIntegerPointer { get; set; }
    public bool CopyCharPointer { get; set; }

    public FunctionCreator()
    {
      this.NegativeIndex = false;
      this.IndexInBounds = false;
      this.AssignStringToStringArray = false;
      this.CalculateSumOfArray = false;
      this.GetElementFromStringArray = false;
      this.StringArrayInitialization = false;
      this.CopyIntegerPointer = false;
      this.CopyCharPointer = false;
    }

    public void WriteFunctions(FileHandler.FileWriter writer)
    {
      if (this.NegativeIndex) WriteNegativeIndexFunction(writer);
      if (this.IndexInBounds)
      {
        if (!this.NegativeIndex) WriteNegativeIndexFunction(writer);
        WriteIndexInBoundsFunction(writer);
      }
      if (this.CalculateSumOfArray) WriteCalculateSumOfArrayFunction(writer);
      if (this.AssignStringToStringArray)
      {
        if (!this.CalculateSumOfArray) WriteCalculateSumOfArrayFunction(writer);
        WriteAssignStringToStringArrayFunction(writer);
      }
      if (this.GetElementFromStringArray) WriteGetElementFromStringArrayFunction(writer);
      if (this.StringArrayInitialization) WriteStringArrayInitializationFunction(writer);
      if (this.CopyIntegerPointer) WriteCopyIntegerPointerFunction(writer);
      if (this.CopyCharPointer) WriteCopyCharPointerFunction(writer);
    }
    private void WriteNegativeIndexFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("int NegativeIndex(int i){");
      writer.WriteLine("if(i<0) goto ERROR;");
      writer.WriteLine("return 1;");
      writer.WriteLine("ERROR:;");
      writer.WriteLine("printf(\"%s\\n\", \"Index is smaller than zero\");");
      writer.WriteLine("return 0;");
      writer.WriteLine("}");
    }
    private void WriteIndexInBoundsFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("int IndexInBounds(int i,int a){");
      writer.WriteLine("if(!NegativeIndex(i)) return 0;");
      writer.WriteLine("if(i>=a) goto ERROR;");
      writer.WriteLine("return 1;");
      writer.WriteLine("ERROR:;");
      writer.WriteLine("printf(\"%s\\n\", \"Index is bigger than allowed\");");
      writer.WriteLine("return 0;");
      writer.WriteLine("}");
    }
    private void WriteCalculateSumOfArrayFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("int CalculateSumOfArray(int s,int* a){");
      writer.WriteLine("int c = 0;");
      writer.WriteLine("int r = 0;");
      writer.WriteLine("START:;");
      writer.WriteLine("if(c==s) goto CONT;");
      writer.WriteLine("r=r+a[c];");
      writer.WriteLine("c=c+1;");
      writer.WriteLine("goto START;");
      writer.WriteLine("CONT:;");
      writer.WriteLine("return r;");
      writer.WriteLine("}");
    }
    private void WriteAssignStringToStringArrayFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("void AssignStringToStringArray(char** a,int i,char* str,int s,int* l){");
      writer.WriteLine("int o=CalculateSumOfArray(s,l);");
      writer.WriteLine("int* ol=malloc(sizeof(int)*s);");
      writer.WriteLine("memcpy(ol,l,sizeof(int)*s);");
      writer.WriteLine("int t=(int)(strlen(str)+1);");
      writer.WriteLine("l[i]=t;");
      writer.WriteLine("t=t-ol[i];");
      writer.WriteLine("t=o+t;");
      writer.WriteLine("char* n=malloc(t);");
      writer.WriteLine("int c=0;");
      writer.WriteLine("int of=0;");
      writer.WriteLine("o=0;");
      writer.WriteLine("START:;");
      writer.WriteLine("if(c==s) goto END;");
      writer.WriteLine("char* sc=malloc(l[c]);");
      writer.WriteLine("memcpy(sc,*a+of,ol[c]);");
      writer.WriteLine("if(c!=i) goto CONT;");
      writer.WriteLine("memcpy(sc,str,l[c]);");
      writer.WriteLine("CONT:;");
      writer.WriteLine("memcpy(n+o,sc,l[c]);");
      writer.WriteLine("c=c+1;");
      writer.WriteLine("o=o+l[c-1];");
      writer.WriteLine("of=of+ol[c-1];");
      writer.WriteLine("goto START;");
      writer.WriteLine("END:;");
      writer.WriteLine("*a=realloc(*a,t);");
      writer.WriteLine("memcpy(*a,n,t);");
      writer.WriteLine("}");
    }
    private void WriteGetElementFromStringArrayFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("void GetElementFromStringArray(char** s,char* a,int i,int* l){");
      writer.WriteLine("int o=0;");
      writer.WriteLine("if(i==0) goto SKIP;");
      writer.WriteLine("int c=0;");
      writer.WriteLine("START:;");
      writer.WriteLine("if(c==i) goto SKIP;");
      writer.WriteLine("o=o+l[c];");
      writer.WriteLine("c=c+1;");
      writer.WriteLine("goto START;");
      writer.WriteLine("SKIP:;");
      writer.WriteLine("memcpy(*s,a+o,l[i]);");
      writer.WriteLine("}");
    }
    private void WriteStringArrayInitializationFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("void InitializeStringArray(char* a,int s,int* l){");
      writer.WriteLine("int i=0;");
      writer.WriteLine("START:;");
      writer.WriteLine("if(i==s) goto SKIP;");
      writer.WriteLine("a[i]='\0';");
      writer.WriteLine("l[i]=1;");
      writer.WriteLine("i=i+1;");
      writer.WriteLine("goto START;");
      writer.WriteLine("SKIP:;");
      writer.WriteLine("}");
    }
    private void WriteCopyIntegerPointerFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("void CopyIntegerPointer(int** dest,int* src,int s){");
      writer.WriteLine("*dest=realloc(*dest,sizeof(int)*s);");
      writer.WriteLine("memcpy(*dest,src,sizeof(int)*s);");
      writer.WriteLine("}");
    }
    private void WriteCopyCharPointerFunction(FileHandler.FileWriter writer)
    {
      writer.WriteLine("void CopyCharPointer(char** dest,char* src,int s){");
      writer.WriteLine("*dest=realloc(*dest,s);");
      writer.WriteLine("memcpy(*dest,src,s);");
      writer.WriteLine("}");
    }
  }
}