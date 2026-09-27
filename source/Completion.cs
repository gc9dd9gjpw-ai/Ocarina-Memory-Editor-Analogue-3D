using System;
using System.Collections.Generic;

// Explicit vanilla flags only. See FLAGS-v2.4.tsv and FUENTES-v2.4.md.
public static class Completion {
 public const string Warning="Experimental en consola. El botón 100 % aplica el catálogo documentado; no certifica todos los eventos de historia, puertas, cofres de rupias ni hacks/Master Quest. Guarda y recarga la partida normal para Scarecrow. Las firmas de Analogue se conservan, no se regeneran.";
 public static readonly string[] Keys={"hearts","inventory","equipment","upgrades","magic","songs","stones","bottles","masks","epona","scarecrow","rewards","dungeons","progress"};
 public static readonly string[] Names={"20/20 corazones · 8 contenedores + 36 piezas reales","Inventario completo · incluye Ice Arrows","Equipo completo · Biggoron Sword","Capacidades máximas · Golden Scale / Giant’s Wallet","Magia, doble magia y doble defensa","Las 12 canciones","6 medallones y 3 piedras espirituales","Cuatro botellas y sus recompensas","Mask of Truth · ventas y deudas saldadas","Epona obtenida","Scarecrow · canción de invocación","Recompensas y minijuegos documentados","Mapas, brújulas y llaves","Progreso persistente documentado"};
 public static CollectionFlag Flag(string n,int table,int flag){return new CollectionFlag(n,false,table+2*(flag>>4),2,1U<<(flag&15));}
 public static readonly CollectionFlag[] Hearts=BuildHearts();
 static CollectionFlag[] BuildHearts(){var a=new List<CollectionFlag>();string[] names={"Gohma","King Dodongo","Barinade","Phantom Ganon","Volvagia","Morpha","Twinrova","Bongo Bongo"};for(int i=0;i<8;i++)a.Add(new CollectionFlag("Contenedor: "+names[i],false,0xd4+(0x11+i)*0x1c+12,4,0x80000000U));return a.ToArray();}
 public static readonly CollectionFlag[] Rewards={
  Flag("Bolos: bolsa de bombas",0xef0,0x11),Flag("Tiro infantil",0xef0,0x0d),Flag("Tiro adulto",0xef0,0x0e),Flag("Tiro a caballo: carcaj",0xef0,0x0f),
  Flag("Teatro: palos",0xef0,0x1e),Flag("Teatro: nueces",0xef0,0x1f),Flag("Mercader: palos",0xef8,0x192),Flag("Mercader: nueces",0xef8,0x193),
  Flag("Ranas: Zelda",0xed4,0xd1),Flag("Ranas: Epona",0xed4,0xd2),Flag("Ranas: Sol",0xed4,0xd3),Flag("Ranas: Saria",0xed4,0xd4),Flag("Ranas: Tiempo",0xed4,0xd5),
  Flag("Skulltulas: premio 10",0xed4,0xda),Flag("Skulltulas: premio 20",0xed4,0xdb),Flag("Skulltulas: premio 30",0xed4,0xdc),Flag("Skulltulas: premio 40",0xed4,0xdd),
  Flag("Carrera Malon: vaca",0xed4,0x1e),Flag("Buceo: escama de plata",0xed4,0x38),
  new CollectionFlag("Pesca adulta: Golden Scale",false,0xec0,4,0x800U)
 };
 public static readonly CollectionFlag[] BottleFlags={Flag("Botella Talon",0xef0,2),Flag("Botella Anju",0xef0,0x0c),Flag("Carta de Ruto recogida",0xed4,0x31),Flag("Carta entregada a Rey Zora",0xed4,0x33)};
 public static readonly CollectionFlag[] MaskFlags={Flag("Venta Keaton",0xef0,0x38),Flag("Venta calavera",0xef0,0x39),Flag("Venta terror",0xef0,0x3a),Flag("Venta conejo",0xef0,0x3b),Flag("Deuda Keaton",0xed4,0x8c),Flag("Deuda calavera",0xed4,0x8d),Flag("Deuda terror",0xed4,0x8e),Flag("Deuda conejo",0xed4,0x8f),Flag("Máscara verdad obtenida",0xef0,0x3f)};
 public static readonly CollectionFlag[] Progress={Flag("Prueba Bosque",0xed4,0xbb),Flag("Prueba Agua",0xed4,0xbc),Flag("Prueba Sombras",0xed4,0xbd),Flag("Prueba Fuego",0xed4,0xbe),Flag("Prueba Luz",0xed4,0xbf),Flag("Prueba Espíritu",0xed4,0xad),Flag("Barrera Torre disipada",0xed4,0xc3),Flag("Espada Maestra obtenida",0xed4,0x45),Flag("Puerta del Tiempo abierta",0xed4,0x4b),Flag("Puente arcoíris",0xed4,0x4d),Flag("Pozo drenado",0xed4,0x67),Flag("Lago restaurado",0xed4,0x69),Flag("Bosque: medallón recibido",0xed4,0x48),Flag("Fuego: medallón recibido",0xed4,0x49),Flag("Agua: medallón recibido",0xed4,0x4a),Flag("Carpintero 1",0xed4,0x90),Flag("Carpintero 2",0xed4,0x91),Flag("Carpintero 3",0xed4,0x92),Flag("Carpintero 4",0xed4,0x93)};
 public static void Put16(byte[] b,int p,int value){b[p]=(byte)(value>>8);b[p+1]=(byte)value;}
 static void Quest(byte[] b,int s,uint mask){MemoryFile.W32(b,s+0xa4,MemoryFile.U32(b,s+0xa4)|mask);}
 static void SetFlags(byte[] b,int s,IEnumerable<CollectionFlag> flags){foreach(var f in flags)f.Set(b,s);}
 public static void Apply(byte[] b,int s,EditOptions o){
  foreach(string key in o.Modules)if(Array.IndexOf(Keys,key)<0)throw new Exception("Módulo desconocido.");
  var extra=new EditOptions();
  if(o.Modules.Contains("inventory")){
   foreach(var i in ItemSpec.All)if(i.Slot<18)extra.Items[i.Slot]=i.Values[i.Values.Length-1];
   SetFlags(b,s,ItemLocations.All);
   foreach(int flag in new[]{0x18,0x19,0x1a})Flag("Hechizo",0xef0,flag).Set(b,s);
  }
  if(o.Modules.Contains("equipment")){foreach(var e in EquipmentSpec.All)extra.Equipment.Add(e.Bit);SetFlags(b,s,ItemLocations.Equipment);SetTrade(b,s,22,0x37);}
  if(o.Modules.Contains("upgrades")){foreach(var u in UpgradeSpec.All)extra.Upgrades[u.Key]=u.MaxLevel;SetFlags(b,s,ItemLocations.Upgrades);}
  if(o.Modules.Contains("magic")){b[s+0x3a]=b[s+0x3c]=b[s+0x3d]=1;b[s+0xcf]=20;b[s+0x32]=2;b[s+0x33]=96;Put16(b,s+0x13f4,96);Put16(b,s+0x13f6,96);}
  if(o.Modules.Contains("songs"))Quest(b,s,0x0003ffc0U);
  if(o.Modules.Contains("stones"))Quest(b,s,0x001c003fU);
  if(o.Modules.Contains("bottles")){
   for(int i=18;i<22;i++){int old=b[s+0x74+i];if(old==255||old==27){b[s+0x74+i]=20;SyncButtons(b,s,i,old,20);}}
   SetFlags(b,s,BottleFlags);if(MemoryFile.U32(b,s+0xebc)<1000)MemoryFile.W32(b,s+0xebc,1000);
  }
  if(o.Modules.Contains("masks")){SetTrade(b,s,23,0x2b);SetFlags(b,s,MaskFlags);}
  if(o.Modules.Contains("epona"))Flag("Epona",0xed4,0x18).Set(b,s);
  if(o.Modules.Contains("scarecrow")){
   // A, C-down repeated; D4/F4. Preserve an already recorded song.
   if(b[s+0x12c5]==0){for(int i=0;i<16;i++){int p=s+0x12c6+i*8;b[p]=(byte)(i<8?(i%2==0?2:5):255);b[p+1]=0;Put16(b,p+2,i<8?20:0);b[p+4]=(byte)(i<8?80:0);b[p+5]=b[p+6]=b[p+7]=0;}b[s+0x12c5]=1;}
   Flag("Scarecrow adulto",0xed4,0x9c).Set(b,s);
  }
  if(o.Modules.Contains("rewards")){
   SetFlags(b,s,Rewards);Quest(b,s,1U<<21); // Stone of Agony
   // Reward dependencies are explicit; hearts/Skulltulas remain separate choices.
   foreach(var u in UpgradeSpec.All)extra.Upgrades[u.Key]=u.MaxLevel;
  }
  if(o.Modules.Contains("dungeons"))SetFlags(b,s,ItemLocations.Dungeons);
  if(o.Modules.Contains("dungeons"))foreach(var d in DungeonSpec.All){if(d.AllowedItems!=0)extra.DungeonItems[d.Index]=d.AllowedItems;if(d.SmallKeys){int[] counts={0,0,0,5,8,6,5,5,3,0,0,9,4,2};int old=b[s+0xbc+d.Index];extra.DungeonKeys[d.Index]=Math.Max(old==255?0:old,counts[d.Index]);}}
  if(o.Modules.Contains("progress")){SetFlags(b,s,Progress);Quest(b,s,1U<<22);}
  if(!extra.Empty)Catalog.Apply(b,s,extra);
  Enforce(b,s,o);
 }
 static void SyncButtons(byte[] b,int s,int slot,int old,int value){foreach(int eq in new[]{0x40,0x4a,0x68})for(int i=0;i<3;i++)if(b[s+eq+4+i]==slot&&b[s+eq+1+i]==old)b[s+eq+1+i]=(byte)value;}
 static void SetTrade(byte[] b,int s,int slot,int value){int old=b[s+0x74+slot];SyncButtons(b,s,slot,old,value);b[s+0x74+slot]=(byte)value;}
 public static void Enforce(byte[] b,int s,EditOptions o){if(o.Modules.Contains("hearts")){SetFlags(b,s,Hearts);Collections.Apply(b,s,new EditOptions{AllHeartPieces=true});Put16(b,s+0x2e,320);Put16(b,s+0x30,320);MemoryFile.W32(b,s+0xa4,MemoryFile.U32(b,s+0xa4)&0x0fffffffU);}}
 public static void Sync(byte[] b,OotDetection d,EditOptions o){
  var flags=new List<CollectionFlag>();if(o.Modules.Contains("hearts")){flags.AddRange(Hearts);flags.AddRange(Collections.All);}if(o.Modules.Contains("inventory"))flags.AddRange(ItemLocations.All);if(o.Modules.Contains("dungeons"))flags.AddRange(ItemLocations.Dungeons);if(o.Modules.Contains("equipment"))flags.AddRange(ItemLocations.Equipment);if(o.Modules.Contains("upgrades"))flags.AddRange(ItemLocations.Upgrades);
  foreach(var f in flags){int p=d.LiveOffset(f);if(p>=0)MemoryFile.W32(b,p,MemoryFile.U32(b,p)|f.Mask);}
 }
 public static void Verify(byte[] b,int s,EditOptions o){
  if(o.Modules.Contains("hearts")){foreach(var f in Hearts)if(!f.Collected(b,s))throw new Exception("Falta un contenedor.");if(OotDetection.U16(b,s+0x2e)!=320||OotDetection.U16(b,s+0x30)!=320||(MemoryFile.U32(b,s+0xa4)>>28)!=0)throw new Exception("Corazones incoherentes.");Collections.Verify(b,s,new EditOptions{AllHeartPieces=true});}
 }
 public static void Describe(MemoryFile m,EditOptions o,List<string> lines){foreach(string k in Keys)if(o.Modules.Contains(k))lines.Add("Módulo: "+Names[Array.IndexOf(Keys,k)]);if(o.Modules.Contains("scarecrow"))lines.Add(m.Bytes[m.Save+0x12c5]==0?"Scarecrow: A, C-abajo, A, C-abajo, A, C-abajo, A, C-abajo. Guardar y recargar partida normal.":"Scarecrow: conserva la canción grabada; habilita reconocimiento adulto.");if(o.Modules.Contains("rewards"))lines.Add("Recompensas: incluye capacidades máximas; consulta FLAGS-v2.4.tsv. Corazones y Skulltulas se eligen aparte.");if(o.Modules.Contains("hearts"))foreach(var f in Hearts)lines.Add(f.Name+": "+(m.Detection.Collected(m.Bytes,f)?"obtenido":"pendiente")+" → obtenido");if(o.Modules.Contains("dungeons"))lines.Add("Llaves pequeñas: reserva vanilla por mazmorra; no marca puertas abiertas.");}
}
