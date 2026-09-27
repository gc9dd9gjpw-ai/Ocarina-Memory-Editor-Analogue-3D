using System;
using System.Collections.Generic;
using System.Text;

// R01/R02: apmd chunk start + 0x10008 (chunk data + 0x10000) is 8 MiB RDRAM.
// Selection uses structure only: no revision addresses, cartridge IDs or code references.
public sealed class CandidateEvidence {
 public uint Address;public int Score;public string Reason;
}
public sealed class OotDetection {
 public int Save,Play,RamStart,RamLength,Scene,Score;
 public List<CandidateEvidence> Candidates;
 public uint SaveAddress,GameId;
 public bool ChecksumMatches;
 public string Cartridge,Region,Revision;
 public string Summary {get{return "Ocarina of Time · "+Region+" · revisión "+Revision+" · Source ID 0x"+GameId.ToString("X8")+" (informativo) · SaveContext 0x"+SaveAddress.ToString("X8")+(Play<0?" · Escena activa no identificada":"");}}
 public static int U16(byte[] b,int p){return (b[p]<<8)|b[p+1];}
 static bool Ptr(uint v,int length){return v>=0x80000000U&&v<0x80000000U+(uint)length&&(v&3)==0;}
 static int At(uint v,int ram){return ram+(int)(v-0x80000000U);}
 static bool LooksLikeSave(byte[] b,int s,int end){
  if(s<0||s+0x1428>end)return false;
  if(Encoding.ASCII.GetString(b,s+28,6)!="ZELDAZ")return false;
  int cap=U16(b,s+0x2e),hp=U16(b,s+0x30);
  if(cap<48||cap>320||hp>cap||U16(b,s+0xd0)>100)return false;
  if(MemoryFile.U32(b,s)>0x613U)return false; // retail entrance table: 0x000..0x613
  if(MemoryFile.U32(b,s+4)>1||MemoryFile.U32(b,s+0x1354)>2||MemoryFile.U32(b,s+0x135c)>3)return false;
  if(b[s+0x3a]>1||b[s+0x3c]>1||b[s+0x3d]>1||b[s+0x3f]>3)return false;
  foreach(var item in ItemSpec.All){int value=b[s+0x74+item.Slot];if(value!=255&&Array.IndexOf(item.Values,value)<0&&!(item.Slot>=18&&value==27))return false;}
  foreach(var u in UpgradeSpec.All)if(u.Read(b,s)>u.MaxLevel)return false;
  if(b[s+0x32]>2||b[s+0x33]>96||b[s+0xcf]>20)return false;
  if(b[s+0x8a]!=255&&(b[s+0x8a]<0x2d||b[s+0x8a]>0x37))return false;
  if(b[s+0x8b]!=255&&(b[s+0x8b]<0x21||b[s+0x8b]>0x2c))return false;
  if(U16(b,s+0x34)>500||b[s+0x3e]>1)return false;
  foreach(var f in Field.All){int v=f.Width==1?b[s+f.Offset]:U16(b,s+f.Offset);if(v<f.Min||v>f.Max)return false;}
  for(int i=0;i<20;i++)if((b[s+0xa8+i]&~7)!=0)return false;
  for(int i=0;i<19;i++)if(b[s+0xbc+i]!=255&&b[s+0xbc+i]>99)return false;
  // Reserved bits are not evidence of corruption. Preserve them untouched.
  return true;
 }
 static bool Hud(int value){return value<=13||value==50||value==52;}
 static int Coherence(byte[] b,int s,out string explanation){
  // Compare relationships and runtime domains, never progress, health magnitude,
  // checksum, cartridge metadata or the candidate's position in memory.
  int score=100;var misses=new List<string>();
  Action<bool,int,string> add=(ok,weight,name)=>{if(ok)score+=weight;else misses.Add(name);};
  add(MemoryFile.U32(b,s+0x135c)==0,4,"modo de juego normal");
  add(MemoryFile.U32(b,s+0x10)<=1,4,"día/noche");
  add(U16(b,s+0x66)<124,4,"escena guardada");
  add(MemoryFile.U32(b,s+0x1360)<=19,4,"capa de escena");
  int respawn=unchecked((int)MemoryFile.U32(b,s+0x1364));
  add(respawn>=-3&&respawn<=3,4,"estado de reaparición");
  add(b[s+0x13c7]<=1,4,"título de escena");
  add(U16(b,s+0x13ce)<=15&&U16(b,s+0x13d2)<=10,4,"temporizadores");
  add(Hud(U16(b,s+0x13e8))&&Hud(U16(b,s+0x13ea))&&Hud(U16(b,s+0x13ee)),4,"interfaz activa");
  add(U16(b,s+0x13f0)<=10&&U16(b,s+0x13f2)<=10,4,"estados de magia");
  int capacity=U16(b,s+0x13f4);
  add((capacity==0||capacity==48||capacity==96)&&U16(b,s+0x13f6)<=96,4,"capacidad de magia viva");
  add(b[s+0x1409]<=2&&b[s+0x140a]<=3&&b[s+0x140c]<=1,4,"opciones de juego");
  add(b[s+0x32]==0||b[s+0x3a]==1,2,"magia adquirida/nivel");
  add(b[s+0x3c]==0||b[s+0x3a]==1,2,"doble magia/adquisición");
  add(b[s+0x32]==0||b[s+0x33]<=capacity,2,"magia actual/capacidad viva");
  add(U16(b,s+0x34)<=Catalog.Capacity(b,s,0x34),2,"rupias/cartera");
  bool ammo=true;foreach(int offset in new[]{0x8c,0x8d,0x8e,0x8f,0x92,0x94,0x9a})
   if(b[s+offset]>Catalog.Capacity(b,s,offset))ammo=false;
  add(ammo,2,"munición/inventario/capacidades");
  add(b[s+0xcf]==0||b[s+0x3d]==1,2,"defensa/adquisición");
  explanation=misses.Count==0?"Coherencia completa":"Coherencia parcial: "+String.Join(", ",misses.ToArray());
  return score;
 }
 static List<int> FindPlay(byte[] b,int ram,int length){
  var list=new List<int>();
  for(int p=ram;p<ram+length-0x12518;p+=16){
   uint address=0x80000000U+(uint)(p-ram);
   if(MemoryFile.U32(b,p+0x790)!=address+0x1e0U||MemoryFile.U32(b,p+0x98)!=1||U16(b,p+0xa4)>=124)continue;
   if(!Ptr(MemoryFile.U32(b,p),length)||!Ptr(MemoryFile.U32(b,p+4),length)||!Ptr(MemoryFile.U32(b,p+8),length))continue;
   int playerList=p+0x1c24+0x0c+2*8;
   uint player=MemoryFile.U32(b,playerList+4);
   if(MemoryFile.U32(b,playerList)!=1||!Ptr(player,length))continue;
   int actor=At(player,ram);if(actor+4>ram+length||U16(b,actor)!=0||b[actor+2]!=2)continue;
   int total=0;bool valid=true;
   for(int cat=0;cat<12;cat++){
    int q=p+0x1c24+0x0c+cat*8;uint n=MemoryFile.U32(b,q),ptr=MemoryFile.U32(b,q+4);
    if(n>255||(n==0?ptr!=0:!Ptr(ptr,length))){valid=false;break;}total+=(int)n;
   }
   if(!valid||total!=b[p+0x1c24+8])continue;
   list.Add(p);
  }
  return list;
 }
 // SRAM caches contain six fixed-size save slots (three files + backups).
 // Identify their container by its header, not by any revision-dependent RAM address.
 // See zeldaret/oot src/code/z_sram.c SLOT_OFFSET and sSramDefaultHeader.
 static bool IsSramSlot(byte[] b,int save,int ram,int end){
  byte[] magic={0x98,0x09,0x10,0x21,0x5a,0x45,0x4c,0x44,0x41};
  for(int slot=0;slot<6;slot++){
   int header=save-0x20-slot*0x1450;
   if(header<ram||header>end-0x8000)continue;
   if(b[header]>3||b[header+1]>1||b[header+2]>2)continue;
   bool match=true;for(int i=0;i<magic.Length;i++)if(b[header+3+i]!=magic[i]){match=false;break;}
   if(match)return true;
  }
  return false;
 }
 public static OotDetection Detect(byte[] b,int payload,int length){
  // payload is the chunk DATA start. The enclosing apmd header is 8 bytes earlier.
  const int ramLength=0x800000;
  if(b==null||payload<8||length<0x10000+ramLength||payload>b.Length-length)
   throw new Exception("Estado truncado: faltan los 8 MB de RDRAM.");
  int ram=payload-8+0x10008,end=ram+ramLength;
  var matches=new List<OotDetection>();var evidence=new List<CandidateEvidence>();
  // Scan every byte, including the last complete context. Never search PNG or trailing state.
  for(int p=ram+0x1c;p<=end-0x1428+0x1c;p++){
   if(b[p]!='Z'||b[p+1]!='E'||b[p+2]!='L'||b[p+3]!='D'||b[p+4]!='A'||b[p+5]!='Z')continue;
   int save=p-0x1c;uint address=0x80000000U+(uint)(save-ram);
   var candidate=new CandidateEvidence{Address=address};evidence.Add(candidate);
   if(IsSramSlot(b,save,ram,end)){candidate.Reason="Copia en contenedor SRAM";continue;}
   if(!LooksLikeSave(b,save,end)){candidate.Reason="Plantilla o estructura inválida";continue;}
   string reason;candidate.Score=Coherence(b,save,out reason);candidate.Reason=reason;
   matches.Add(new OotDetection{Save=save,RamStart=ram,RamLength=ramLength,
    SaveAddress=address,Play=-1,Scene=-1,Score=candidate.Score});
  }
  if(matches.Count==0)throw new Exception("No se identifica un SaveContext vivo compatible tras descartar plantillas, copias SRAM y estructuras inválidas.");
  matches.Sort((a,c)=>c.Score.CompareTo(a.Score));
  if(matches.Count>1&&matches[0].Score==matches[1].Score)
   throw new Exception("Persisten candidatos indistinguibles por coherencia tras descartar plantillas y SRAM (puntuación "+matches[0].Score+"). No se elegirá por dirección ni checksum.");
  var result=matches[0];result.Candidates=evidence;
  // Independent live-scene discovery is used only to synchronize scene-local rewards.
  var plays=FindPlay(b,ram,ramLength);
  if(plays.Count==1){result.Play=plays[0];result.Scene=U16(b,result.Play+0xa4);}
  string cartridge=Encoding.ASCII.GetString(b,payload+0x5b,4);
  result.Cartridge=cartridge;result.Region="región desconocida";result.Revision="desconocida";
  if(cartridge.Substring(1,2)=="ZL"){
   char region=cartridge[3];result.Region=region=='P'?"PAL (Europa/Australia)":region=='E'?"NTSC (América)":region=='J'?"NTSC (Japón)":"región desconocida";
   result.Revision="1."+b[payload+0x5f];
  }
  result.GameId=MemoryFile.U32(b,payload+24);
  // A live checksum is diagnostic only, never a selection/rejection criterion.
  result.ChecksumMatches=MemoryFile.Checksum(b,result.Save)==U16(b,result.Save+0x1352);
  return result;
 }
 public void RequireSceneFor(EditOptions options){
  if(Play>=0)return;
  if(options.AllHeartPieces||options.Modules.Contains("hearts")||options.Modules.Contains("inventory")||
     options.Modules.Contains("dungeons")||options.Modules.Contains("equipment")||options.Modules.Contains("upgrades"))
   throw new Exception("SaveContext detectado, pero no se identifica una escena activa única para sincronizar estas recompensas. Crea otra Memory durante la partida, con Link visible y fuera de transiciones.");
 }
 public int LiveOffset(CollectionFlag f){
  // SavedSceneFlags start at +D4, stride 1C; chest +0, collect +C.
  if(Play<0||f.Skull||f.Offset<0xd4||f.Offset>=0xe64)return -1;
  int relative=f.Offset-0xd4,scene=relative/0x1c,field=relative%0x1c;
  if(scene!=Scene||f.Width!=4)return -1;
  if(field==0)return Play+0x1d28+0x10;
  if(field==12)return Play+0x1d28+0x1c;
  return -1;
 }
 public void SyncCollections(byte[] b,EditOptions options){
  if(!options.AllHeartPieces)return;
  foreach(var f in Collections.All){int p=LiveOffset(f);if(p>=0)MemoryFile.W32(b,p,MemoryFile.U32(b,p)|f.Mask);}
 }
 public bool Collected(byte[] b,CollectionFlag f){
  if(f.Collected(b,Save))return true;
  int p=LiveOffset(f);return p>=0&&(MemoryFile.U32(b,p)&f.Mask)==f.Mask;
 }
}


