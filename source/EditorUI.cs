using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public sealed class Choice {
 public string Text;public int Value;public Choice(string text,int value){Text=text;Value=value;}public override string ToString(){return Text;}
}
public partial class Editor:Form {
 MemoryFile memory;
 Dictionary<int,NumericUpDown> inputs=new Dictionary<int,NumericUpDown>();
 Dictionary<int,Label> limits=new Dictionary<int,Label>();
 Dictionary<string,ComboBox> upgrades=new Dictionary<string,ComboBox>();
 Dictionary<int,ComboBox> items=new Dictionary<int,ComboBox>();
 Dictionary<int,CheckBox> equipment=new Dictionary<int,CheckBox>();
 Label status,filename;Button save,fill,reset,maxUpgrades,addItems,addEquipment;TextBox review;TabControl tabs;
 bool loading,dirty;
 static Label LabelText(string s){return new Label{Text=s,AutoSize=true,Anchor=AnchorStyles.Left,Margin=new Padding(5,5,5,5)};}
 static Button ButtonText(string s,int width){return new Button{Text=s,Width=width,Height=34,Margin=new Padding(0,4,10,4)};}
 static TableLayoutPanel Grid(){var g=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,Padding=new Padding(10)};g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,40));g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,60));return g;}
 static ComboBox Combo(){return new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=380,Enabled=false,Margin=new Padding(5),Anchor=AnchorStyles.Left|AnchorStyles.Right};}
 TabPage Page(string name,string help){var p=new TabPage(name){BackColor=Color.White,Padding=new Padding(10)};var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};layout.RowStyles.Add(new RowStyle(SizeType.Absolute,48));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.Controls.Add(new Label{Text=help,Dock=DockStyle.Fill,ForeColor=Color.FromArgb(70,85,98)},0,0);var panel=new Panel{Dock=DockStyle.Fill,AutoScroll=true};layout.Controls.Add(panel,0,1);p.Controls.Add(layout);tabs.TabPages.Add(p);return p;}
 Panel Body(TabPage p){return (Panel)((TableLayoutPanel)p.Controls[0]).GetControlFromPosition(0,1);}
 public Editor(){
  Text="Ocarina · Editor de Memories 3.0.0";ClientSize=new Size(960,780);MinimumSize=new Size(900,760);Font=new Font("Segoe UI",10);BackColor=Color.FromArgb(245,247,249);AutoScaleMode=AutoScaleMode.Dpi;
  var root=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(22),ColumnCount=1,RowCount=7};Controls.Add(root);
  root.RowStyles.Add(new RowStyle(SizeType.Absolute,44));root.RowStyles.Add(new RowStyle(SizeType.Absolute,28));root.RowStyles.Add(new RowStyle(SizeType.Absolute,50));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,48));root.RowStyles.Add(new RowStyle(SizeType.Absolute,48));root.RowStyles.Add(new RowStyle(SizeType.Absolute,32));
  root.Controls.Add(new Label{Text="Ocarina · Memories",Font=new Font("Segoe UI",21,FontStyle.Bold),AutoSize=true},0,0);
  root.Controls.Add(new Label{Text="Recursos, inventario y coleccionables · Ocarina de distintas regiones · detección automática",AutoSize=true},0,1);
  var openbar=new FlowLayoutPanel{Dock=DockStyle.Fill};var open=ButtonText("Abrir Memory…",150);open.Click+=(s,e)=>OpenMemory();filename=new Label{Text="Ningún archivo abierto",Width=690,Height=40,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft};openbar.Controls.Add(open);openbar.Controls.Add(filename);root.Controls.Add(openbar,0,2);
  tabs=new TabControl{Dock=DockStyle.Fill};root.Controls.Add(tabs,0,3);
  var resources=Page("Recursos","Contador de Skulltulas: no marca recogidas. Usa Coleccionables para eso.\nCapacidad = con las mejoras seleccionadas; máximo = límite normal del juego.");
  var grid=Grid();grid.ColumnCount=3;grid.ColumnStyles.Clear();grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,22));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45));int row=0;
  foreach(Field f in Field.All){grid.RowStyles.Add(new RowStyle(SizeType.Absolute,31));grid.Controls.Add(LabelText(f.Name),0,row);var n=new NumericUpDown{Minimum=f.Min/f.Scale,Maximum=f.Max/f.Scale,DecimalPlaces=f.Scale==16?4:0,Increment=f.Scale==16?0.25m:1,Width=135,Enabled=false};n.ValueChanged+=(s,e)=>Changed();inputs.Add(f.Offset,n);grid.Controls.Add(n,1,row);var hint=LabelText("Máximo: "+(f.Max/f.Scale));limits.Add(f.Offset,hint);grid.Controls.Add(hint,2,row++);}Body(resources).Controls.Add(grid);
  var improvement=Page("Mejoras · experimental","Aumenta bolsas, cartera, fuerza y buceo. Las mejoras de munición incluyen el objeto correspondiente.\nNo se rebajan mejoras existentes. Después puedes pulsar Rellenar recursos.");
  var upgrid=Grid();row=0;foreach(var u in UpgradeSpec.All){upgrid.Controls.Add(LabelText(u.Name),0,row);var c=Combo();c.SelectedIndexChanged+=(s,e)=>Changed();upgrades.Add(u.Key,c);upgrid.Controls.Add(c,1,row++);}maxUpgrades=ButtonText("Mejoras al máximo",210);maxUpgrades.Enabled=false;maxUpgrades.Click+=(s,e)=>SetMaxUpgrades();upgrid.Controls.Add(maxUpgrades,1,row);Body(improvement).Controls.Add(upgrid);
  var inventory=Page("Objetos · experimental","Añade objetos o mejora ocarina y gancho. No desbloquea canciones ni eventos de la historia.\nLas botellas permiten elegir contenido. Las ranuras especiales se conservan.");
  var itemgrid=Grid();row=0;foreach(var i in ItemSpec.All){itemgrid.Controls.Add(LabelText(i.Name),0,row);var c=Combo();c.SelectedIndexChanged+=(s,e)=>Changed();items.Add(i.Slot,c);itemgrid.Controls.Add(c,1,row++);}addItems=ButtonText("Añadir objetos básicos",230);addItems.Enabled=false;addItems.Click+=(s,e)=>SetAllItems();itemgrid.Controls.Add(addItems,1,row);Body(inventory).Controls.Add(itemgrid);
  var equipPage=Page("Equipo · experimental","Añade espadas, escudos, túnicas y botas. Lo que ya tienes se conserva.\nElige después el equipo en el menú del juego; siguen aplicándose las restricciones por edad.");
  var equipGrid=Grid();row=0;foreach(var e in EquipmentSpec.All){var c=new CheckBox{Text=e.Name,AutoSize=true,Enabled=false,Margin=new Padding(8)};c.CheckedChanged+=(s,ev)=>Changed();equipment.Add(e.Bit,c);equipGrid.Controls.Add(c,row%2,row/2);row++;}addEquipment=ButtonText("Añadir todo el equipo",230);addEquipment.Enabled=false;addEquipment.Click+=(s,e)=>{loading=true;foreach(var c in equipment.Values)if(c.Enabled)c.Checked=true;loading=false;Changed();};equipGrid.Controls.Add(addEquipment,0,7);Body(equipPage).Controls.Add(equipGrid);
  BuildDungeonPage();
  BuildCollectionsPage();
  BuildCompletionPage();
  var reviewPage=Page("Revisión","Revisa los cambios antes de guardar. Las dependencias necesarias se aplican automáticamente.\nSe conserva A3DSignature; su autenticidad no se puede recalcular. CRC y checksum sí se verifican.");review=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,BackColor=Color.White,BorderStyle=BorderStyle.None};Body(reviewPage).Controls.Add(review);
  var actions=new FlowLayoutPanel{Dock=DockStyle.Fill};fill=ButtonText("Rellenar recursos",175);fill.Enabled=false;fill.Click+=(s,e)=>FillResources();reset=ButtonText("Restablecer todo",175);reset.Enabled=false;reset.Click+=(s,e)=>Populate();save=ButtonText("Guardar copia…",175);save.Enabled=false;save.Click+=(s,e)=>SaveMemory();actions.Controls.AddRange(new Control[]{fill,reset,save});root.Controls.Add(actions,0,4);
  root.Controls.Add(new Label{Text="Las opciones nuevas son experimentales: el archivo se verifica, su efecto en consola aún no.\nGuarda siempre una copia. No incluye vida infinita, atravesar paredes ni completar misiones.",Dock=DockStyle.Fill,ForeColor=Color.FromArgb(70,85,98)},0,5);
  status=new Label{Text="Abre una Memory para empezar.",Dock=DockStyle.Fill,AutoEllipsis=true};root.Controls.Add(status,0,6);
  FormClosing+=(s,e)=>{if(!MayDiscard())e.Cancel=true;};
 }
 bool MayDiscard(){return !dirty||MessageBox.Show(this,"Hay cambios sin guardar. ¿Descartarlos?","Cambios pendientes",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;}
 void OpenMemory(){if(!MayDiscard())return;using(var d=new OpenFileDialog{Filter="Memories PNG|*.png",Title="Abrir Memory de Ocarina"}){if(d.ShowDialog()!=DialogResult.OK)return;try{LoadMemory(d.FileName);}catch(Exception ex){MessageBox.Show(this,ex.Message,"No se pudo abrir",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}}
 public void LoadMemory(string path){var next=new MemoryFile(path);foreach(Field f in Field.All){int v=next.Read(f.Offset,f.Width);if(v<f.Min||v>f.Max)throw new Exception("Valor no compatible en "+f.Name+". No se ha abierto el archivo.");}memory=next;filename.Text=Path.GetFileName(path);Populate();save.Enabled=fill.Enabled=reset.Enabled=maxUpgrades.Enabled=addItems.Enabled=addEquipment.Enabled=true;status.Text=memory.Detection.Summary+" · Partida activa reconocida. Original protegido."+(memory.Detection.ChecksumMatches?"":" Checksum RAM desactualizado; se recalculará.");}
 void Populate(){loading=true;
  foreach(Field f in Field.All){inputs[f.Offset].Value=memory.Read(f.Offset,f.Width)/f.Scale;inputs[f.Offset].Enabled=true;}
  foreach(var u in UpgradeSpec.All){var c=upgrades[u.Key];int level=u.Read(memory.Bytes,memory.Save);c.Items.Clear();c.Items.Add(new Choice("Conservar · "+u.Describe(level),-1));for(int n=level+1;n<=u.MaxLevel;n++)c.Items.Add(new Choice(u.Describe(n),n));c.SelectedIndex=0;c.Enabled=c.Items.Count>1;}
  foreach(var i in ItemSpec.All){var c=items[i.Slot];int value=memory.Read(0x74+i.Slot,1);int index=Array.IndexOf(i.Values,value);c.Items.Clear();c.Items.Add(new Choice("Conservar · "+i.Describe(value),-1));if(value==255||index>=0)for(int n=0;n<i.Values.Length;n++)if(i.Values[n]!=value&&(i.Slot>=18||n>index))c.Items.Add(new Choice(i.Names[n],i.Values[n]));c.SelectedIndex=0;c.Enabled=c.Items.Count>1;}
  foreach(var e in EquipmentSpec.All){bool owned=e.Owned(memory.Bytes,memory.Save);equipment[e.Bit].Checked=owned;equipment[e.Bit].Enabled=!owned;}
  PopulateDungeons();
  PopulateCollections();
  foreach(var c in moduleChecks.Values){c.Checked=false;c.Enabled=true;}completeAll.Enabled=true;
  loading=false;dirty=false;UpdateLimits();
 }
 EditOptions Options(){var result=new EditOptions();foreach(var p in upgrades){var c=p.Value.SelectedItem as Choice;if(c!=null&&c.Value>=0)result.Upgrades.Add(p.Key,c.Value);}foreach(var p in items){var c=p.Value.SelectedItem as Choice;if(c!=null&&c.Value>=0)result.Items.Add(p.Key,c.Value);}foreach(var p in equipment)if(p.Value.Enabled&&p.Value.Checked)result.Equipment.Add(p.Key);CollectDungeons(result);result.AllSkulltulas=allSkulltulas.Checked;result.AllHeartPieces=allHeartPieces.Checked;foreach(var c in moduleChecks)if(c.Value.Checked)result.Modules.Add(c.Key);if(result.Modules.Contains("hearts"))result.AllHeartPieces=true;return result;}
 Dictionary<int,int> Values(){var result=new Dictionary<int,int>();foreach(Field f in Field.All){decimal v=inputs[f.Offset].Value*f.Scale;if(v!=Decimal.Truncate(v))throw new Exception("Usa incrementos de 0,0625 corazones.");int n=(int)v;if(n!=memory.Read(f.Offset,f.Width))result.Add(f.Offset,n);}return result;}
 byte[] Preview(){byte[] b=(byte[])memory.Bytes.Clone();Catalog.Apply(b,memory.Save,Options());int hp=(int)(inputs[0x2e].Value*16);b[memory.Save+46]=(byte)(hp>>8);b[memory.Save+47]=(byte)hp;Completion.Enforce(b,memory.Save,Options());return b;}
 int CurrentCapacity(int offset){return Catalog.Capacity(Preview(),memory.Save,offset);}
 void Changed(){if(loading||memory==null)return;dirty=true;UpdateLimits();}
 void UpdateLimits(){if(memory==null)return;try{UpdateCollectionLabels();byte[] b=Preview();var lines=Catalog.Describe(memory,Options());foreach(Field f in Field.All){decimal cap=Catalog.Capacity(b,memory.Save,f.Offset)/f.Scale;bool over=inputs[f.Offset].Value>cap;limits[f.Offset].Text=(f.Offset==0x2e||f.Offset==0xd0)?"Máximo: "+(f.Max/f.Scale):"Capacidad: "+cap+" · Máximo: "+(f.Max/f.Scale)+(over?" · Excede":"");limits[f.Offset].ForeColor=over?Color.Firebrick:Color.FromArgb(70,85,98);if(inputs[f.Offset].Value!=memory.Read(f.Offset,f.Width)/f.Scale)lines.Add(f.Name+": "+(memory.Read(f.Offset,f.Width)/f.Scale)+" → "+inputs[f.Offset].Value);}
   if(lines.Count>0){byte[] planned=memory.Patch(Values(),Options());lines.Add("\r\nCambios exactos (offset de archivo: antes → después):");for(int k=0;k<planned.Length;k++)if(planned[k]!=memory.Bytes[k])lines.Add("0x"+k.ToString("X8")+": "+memory.Bytes[k].ToString("X2")+" → "+planned[k].ToString("X2"));}
   review.Text=lines.Count==0?"No hay cambios seleccionados.":Completion.Warning+Environment.NewLine+Environment.NewLine+(memory.Detection.ChecksumMatches?"Checksum de origen correcto.":"Checksum RAM de origen desactualizado: se recalculará al guardar.")+Environment.NewLine+Environment.NewLine+String.Join(Environment.NewLine,lines.ToArray())+Environment.NewLine+Environment.NewLine+"Los objetos añadidos pueden requerir munición y equiparse desde el menú. Abrir el menú y volver a equipar también refresca los iconos.";
  }catch(Exception ex){status.Text=ex.Message;review.Text=ex.Message;}}
 void FillResources(){if(memory==null)return;try{byte[] b=Preview();loading=true;foreach(Field f in Field.All)if(f.Offset!=0xd0&&f.Offset!=0x2e)inputs[f.Offset].Value=Catalog.Capacity(b,memory.Save,f.Offset)/f.Scale;loading=false;Changed();status.Text="Recursos rellenados con las mejoras seleccionadas.";}catch(Exception ex){loading=false;MessageBox.Show(this,ex.Message);}}
 void SetMaxUpgrades(){loading=true;foreach(var p in upgrades)if(p.Value.Enabled)p.Value.SelectedIndex=p.Value.Items.Count-1;loading=false;Changed();status.Text="Mejoras al máximo seleccionadas; revisa y rellena recursos si lo deseas.";}
 void SetAllItems(){loading=true;foreach(var p in items)if(p.Key<18&&p.Value.Enabled)p.Value.SelectedIndex=p.Value.Items.Count-1;loading=false;Changed();status.Text="Objetos básicos seleccionados. Botellas e intercambios se conservan.";}
 void SaveMemory(){try{var values=Values();var options=Options();if(values.Count==0&&options.Empty){status.Text="No hay cambios que guardar.";return;}memory.Patch(values,options);tabs.SelectedIndex=tabs.TabCount-1;using(var d=new SaveFileDialog{Filter="Memory PNG|*.png",FileName=Path.GetFileName(memory.PathName),Title="Guardar en otra carpeta; el original se conserva",OverwritePrompt=true}){if(d.ShowDialog()!=DialogResult.OK)return;memory.SaveCopy(d.FileName,values,options);dirty=false;status.Text="Copia guardada; checksum OoT y CRC verificados. Original intacto.";MessageBox.Show(this,"Copia guardada.\n\nCambia de zona tras cargar, guarda dentro del juego y vuelve a cargar esa partida normal. Scarecrow requiere esta recarga.\n\nLas opciones nuevas son experimentales. Selecciona el equipo desde el menú del juego; vuelve a equipar objetos si el icono no se ha actualizado.","Listo",MessageBoxButtons.OK,MessageBoxIcon.Information);}}catch(Exception ex){MessageBox.Show(this,ex.Message,"No se guardó la copia",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
 [STAThread]public static void Main(){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new Editor());}
}
