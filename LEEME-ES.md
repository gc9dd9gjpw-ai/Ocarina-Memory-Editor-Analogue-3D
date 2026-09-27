# Ocarina Memory Editor para Analogue 3D — v3.0.0

Editor para Windows de Memories de **The Legend of Zelda: Ocarina of Time** creadas por Analogue 3D / 3DOS.

> Utilidad fan independiente. No contiene ROMs, datos del juego de Nintendo, firmware ni software de Analogue. No está afiliada ni respaldada por Nintendo o Analogue.

## Funcionamiento

Abre el PNG Memory original de Analogue 3D, localiza estructuralmente el SaveContext vivo de Ocarina of Time, permite modificar los campos compatibles, recalcula el checksum de OoT y el CRC del chunk `apmd`, y guarda **una copia nueva**. El programa no sobrescribe el Memory de origen.

El detector no selecciona la partida mediante una dirección SaveContext fija, Source/Cart ID, build, región, checksum válido ni una regla de «segundo ZELDAZ». Busca candidatos en la RDRAM capturada y exige un único SaveContext vivo estructuralmente coherente. Si la detección es ambigua, no escribe.

## Versiones verificadas con Memories reales

- NTSC-U 1.0
- NTSC-U 1.1
- NTSC-U 1.2
- PAL 1.0
- PAL 1.1
- Master Quest PAL / GameCube

Son pruebas de regresión, no una lista blanca. No se garantiza el funcionamiento con randomizers o ROM hacks que alteren estructuras, valores o la lógica del mundo.

## Uso seguro

1. Conserva siempre el Memory original.
2. Abre el `.png` original generado por Analogue 3D; no una imagen reexportada.
3. Revisa los cambios antes de guardar.
4. Guarda con otro nombre o ruta. El editor no sobrescribe archivos existentes.
5. La copia se vuelve a abrir y verificar antes de terminar.
6. Tras cargarla en la consola, cambia de zona y guarda normalmente dentro del juego cuando corresponda.

Un SaveContext vivo puede tener el checksum desactualizado en el instante de la captura; por eso no se usa como requisito para detectar la partida viva. La copia creada sí debe superar las comprobaciones de integridad del editor.

## Ejecutar

Extrae el ZIP y abre `MemoryEditor.exe`. Requiere Windows y .NET Framework 4.x.

El código C# está incluido en `source/` y la documentación técnica en `docs/`.

## Distribución

Descarga gratuita. La página de itch.io puede habilitar donaciones opcionales.

## Licencia

Todavía no se ha elegido una licencia open source. El código se incluye por transparencia y reproducibilidad. No redistribuyas versiones modificadas como lanzamientos oficiales sin permiso del autor.
