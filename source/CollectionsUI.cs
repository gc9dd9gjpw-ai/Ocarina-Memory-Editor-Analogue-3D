using System;
using System.Drawing;
using System.Windows.Forms;

public partial class Editor {
 CheckBox allSkulltulas,allHeartPieces;
 Label skullCount,heartCount;
 DataGridView collectionList;
 decimal previousTokenCount;
 void BuildCollectionsPage(){
  tabs.Multiline=true;
  var page=Page("Coleccionables","Lee las marcas individuales de recogida, separadas de los contadores. Opciones experimentales.\nSincroniza también las marcas del escenario activo. Cambia de zona después de cargar la copia.");
  var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=6};
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute,30));
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute,30));
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute,62));
  layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
  skullCount=new Label{Text="Skulltulas recogidas: — / 100",Dock=DockStyle.Fill};
  heartCount=new Label{Text="Piezas de corazón recogidas: — / 36",Dock=DockStyle.Fill};
  allSkulltulas=new CheckBox{Text="Marcar las 100 Skulltulas como recogidas y poner contador en 100",Dock=DockStyle.Fill,Enabled=false};
  allHeartPieces=new CheckBox{Text="Marcar las 36 piezas de corazón como recogidas",Dock=DockStyle.Fill,Enabled=false};
  allSkulltulas.CheckedChanged+=(s,e)=>{
   if(loading||memory==null)return;
   loading=true;
   if(allSkulltulas.Checked){previousTokenCount=inputs[0xd0].Value;inputs[0xd0].Value=100;}
   else inputs[0xd0].Value=previousTokenCount;
   inputs[0xd0].Enabled=!allSkulltulas.Checked;
   loading=false;Changed();
  };
  allHeartPieces.CheckedChanged+=(s,e)=>Changed();
  layout.Controls.Add(skullCount,0,0);layout.Controls.Add(allSkulltulas,0,1);
  layout.Controls.Add(heartCount,0,2);layout.Controls.Add(allHeartPieces,0,3);
  layout.Controls.Add(new Label{Text="Las piezas no cambian los corazones máximos: ajústalos en Recursos si lo necesitas.\nIncluye las recompensas de minijuegos; excluye los 8 contenedores de jefes.\nLa lista muestra el nombre de cada ubicación del catálogo original.",Dock=DockStyle.Fill,ForeColor=Color.FromArgb(70,85,98)},0,4);
  collectionList=new DataGridView{Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=Color.White,BorderStyle=BorderStyle.FixedSingle,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false};
  collectionList.Columns.Add("kind","Grupo");collectionList.Columns[0].FillWeight=22;
  collectionList.Columns.Add("name","Ubicación");collectionList.Columns[1].FillWeight=56;
  collectionList.Columns.Add("before","Antes");collectionList.Columns[2].FillWeight=11;
  collectionList.Columns.Add("after","Después");collectionList.Columns[3].FillWeight=11;
  foreach(DataGridViewColumn c in collectionList.Columns)c.SortMode=DataGridViewColumnSortMode.NotSortable;
  layout.Controls.Add(collectionList,0,5);Body(page).Controls.Add(layout);
 }
 void PopulateCollections(){
  allSkulltulas.Checked=allHeartPieces.Checked=false;
  allSkulltulas.Enabled=allHeartPieces.Enabled=true;
  previousTokenCount=memory.Read(0xd0,2);
  collectionList.Rows.Clear();
  foreach(var f in Collections.All)collectionList.Rows.Add(f.Skull?"Skulltula":"Pieza de corazón",f.Name,memory.Detection.Collected(memory.Bytes,f)?"Sí":"No","");
  UpdateCollectionLabels();
 }
 void UpdateCollectionLabels(){
  if(memory==null)return;
  int gs=memory.CollectedCount(true),hp=memory.CollectedCount(false);
  skullCount.Text="Skulltulas recogidas: "+gs+" / 100"+(allSkulltulas.Checked?"   →   100 / 100":"")+"     · Contador original: "+memory.Read(0xd0,2);
  heartCount.Text="Piezas de corazón recogidas: "+hp+" / 36"+(allHeartPieces.Checked?"   →   36 / 36":"");
  for(int i=0;i<Collections.All.Length;i++){
   var f=Collections.All[i];bool before=memory.Detection.Collected(memory.Bytes,f),after=before||(f.Skull?allSkulltulas.Checked:allHeartPieces.Checked);
   collectionList.Rows[i].Cells[3].Value=after?"Sí":"No";
   collectionList.Rows[i].DefaultCellStyle.BackColor=after&&!before?Color.FromArgb(230,246,233):Color.White;
  }
 }
}

