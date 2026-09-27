using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class MemoryFile {
 public OotDetection Detection;
 public byte[] Bytes; public int Payload, Length, Save; public string PathName;
 public static ushort Checksum(byte[] b,int s){uint sum=0;for(int i=0;i<0x1352;i+=2)sum+=(uint)((b[s+i]<<8)|b[s+i+1]);return (ushort)sum;}
 static uint[] table=MakeTable();
 static uint[] MakeTable(){uint[] t=new uint[256]; for(uint i=0;i<256;i++){uint c=i;for(int k=0;k<8;k++)c=(c&1)!=0?0xedb88320U^(c>>1):c>>1;t[i]=c;}return t;}
 public static uint CRC(byte[] b,int start,int count){uint c=0xffffffff;for(int i=start;i<start+count;i++)c=table[(c^b[i])&255]^(c>>8);return c^0xffffffff;}
 public static uint U32(byte[] b,int p){return ((uint)b[p]<<24)|((uint)b[p+1]<<16)|((uint)b[p+2]<<8)|b[p+3];}
 public static void W32(byte[] b,int p,uint v){b[p]=(byte)(v>>24);b[p+1]=(byte)(v>>16);b[p+2]=(byte)(v>>8);b[p+3]=(byte)v;}
 public int CollectedCount(bool skull){int n=0;foreach(var f in Collections.All)if(f.Skull==skull&&Detection.Collected(Bytes,f))n++;return n;}
 public int Read(int offset,int width){int p=Save+offset;return width==1?Bytes[p]:(Bytes[p]<<8)|Bytes[p+1];}
 public MemoryFile(string path):this(File.ReadAllBytes(path)){PathName=System.IO.Path.GetFullPath(path);}
 public MemoryFile(byte[] data){
  Bytes=(byte[])data.Clone();if(data.Length<8||BitConverter.ToString(data,0,8)!="89-50-4E-47-0D-0A-1A-0A")throw new Exception("No es una Memory PNG.");
  int p=8;bool end=false,header=false,image=false;int found=0;
  while(p+12<=data.Length){string type=Encoding.ASCII.GetString(data,p+4,4);
   if(end && data.Length-p==96 && Encoding.ASCII.GetString(data,p,16)=="Analogue3DAnalog")break;
   uint n=U32(data,p);if(n>data.Length-p-12)throw new Exception("Bloque truncado o formato no compatible.");int len=(int)n;
   if(CRC(data,p+4,len+4)!=U32(data,p+8+len))throw new Exception("CRC incorrecto en "+type+". No se editará este archivo.");
   if(p==8&&(type!="IHDR"||len!=13))throw new Exception("Falta IHDR válido.");
   if(type=="IHDR"){if(header||len!=13||U32(data,p+8)==0||U32(data,p+12)==0)throw new Exception("IHDR inválido.");header=true;}
   if(type=="IDAT"){if(end)throw new Exception("IDAT después de IEND.");image=true;}
   if(type=="IEND"&&(end||len!=0))throw new Exception("IEND inválido.");
   if(type=="apmd"){Payload=p+8;Length=len;found++;}if(type=="IEND")end=true;p+=12+len;
  }
  if(p!=data.Length && !(data.Length-p==96 && Encoding.ASCII.GetString(data,p,16)=="Analogue3DAnalog"))throw new Exception("Datos finales no reconocidos.");
  if(!header||!image||!end||found!=1||Length!=0x810000)throw new Exception("No se reconoce el estado de Analogue 3D.");
  string magic=Encoding.ASCII.GetString(data,Payload,14);
  if(magic!="Ultra-FP64_R01"&&magic!="Ultra-FP64_R02")throw new Exception("Versión de Memory no compatible.");
  Detection=OotDetection.Detect(data,Payload,Length);
  Save=Detection.Save;
 }
 public byte[] Patch(Dictionary<int,int> changes){return Patch(changes,new EditOptions());}
 public byte[] Patch(Dictionary<int,int> changes,EditOptions options){
  Detection.RequireSceneFor(options);
  byte[] b=(byte[])Bytes.Clone();Catalog.Apply(b,Save,options);foreach(var pair in changes){Field f=Array.Find(Field.All,x=>x.Offset==pair.Key);if(f==null||pair.Value<f.Min||pair.Value>f.Max)throw new Exception("Valor fuera del rango permitido.");int pos=Save+pair.Key;if(f.Width==2){b[pos]=(byte)(pair.Value>>8);b[pos+1]=(byte)pair.Value;}else b[pos]=(byte)pair.Value;}
  Completion.Enforce(b,Save,options);
  Detection.SyncCollections(b,options);
  Completion.Sync(b,Detection,options);
  Collections.Verify(b,Save,options);
  int cap=(b[Save+46]<<8)|b[Save+47],hp=(b[Save+48]<<8)|b[Save+49];if(hp>cap)throw new Exception("La vida actual no puede superar la capacidad de corazones.");
  ushort checksum=Checksum(b,Save);b[Save+0x1352]=(byte)(checksum>>8);b[Save+0x1353]=(byte)checksum;
  W32(b,Payload+Length,CRC(b,Payload-4,Length+4));var verified=new MemoryFile(b);
  if(!verified.Detection.ChecksumMatches)throw new Exception("Checksum OoT incorrecto después de editar.");
  Completion.Verify(b,Save,options);
  if(verified.Save!=Save||verified.Detection.Play!=Detection.Play)throw new Exception("La identificación cambió después de editar; no se guardará.");
  return b;
 }
 public void SaveCopy(string path,Dictionary<int,int> changes){SaveCopy(path,changes,new EditOptions());}
 public void SaveCopy(string path,Dictionary<int,int> changes,EditOptions options){
  if(String.Equals(System.IO.Path.GetFullPath(path),PathName,StringComparison.OrdinalIgnoreCase))throw new Exception("Elige otra carpeta o nombre: se conserva el original.");
  if(File.Exists(path))throw new Exception("El archivo de destino ya existe. Elige un nombre nuevo.");
  byte[] b=Patch(changes,options);string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
  try {
   using(var stream=new FileStream(temp,FileMode.CreateNew,FileAccess.Write)){stream.Write(b,0,b.Length);stream.Flush();}
   var check=new MemoryFile(temp);Collections.Verify(check.Bytes,check.Save,options);Completion.Verify(check.Bytes,check.Save,options);
   if(!check.Detection.ChecksumMatches||!System.Linq.Enumerable.SequenceEqual(b,check.Bytes))throw new Exception("La copia guardada no coincide.");
   File.Move(temp,path);
  } finally {if(File.Exists(temp))File.Delete(temp);}

 }
}
public class Field {
 public string Name;public int Offset,Width,Min,Max; public decimal Scale;
 public Field(string n,int o,int w,int lo,int hi,decimal scale){Name=n;Offset=o;Width=w;Min=lo;Max=hi;Scale=scale;}
 public static Field[] All={new Field("Contador Skulltulas",0xd0,2,0,100,1),new Field("Rupias",0x34,2,0,500,1),new Field("Corazones máximos",0x2e,2,48,320,16),new Field("Vida actual (corazones)",0x30,2,0,320,16),new Field("Magia actual",0x33,1,0,96,1),new Field("Palos Deku",0x8c,1,0,30,1),new Field("Nueces Deku",0x8d,1,0,40,1),new Field("Bombas",0x8e,1,0,40,1),new Field("Flechas",0x8f,1,0,50,1),new Field("Semillas Deku",0x92,1,0,50,1),new Field("Bombchus",0x94,1,0,50,1),new Field("Judías mágicas",0x9a,1,0,10,1)};
}

