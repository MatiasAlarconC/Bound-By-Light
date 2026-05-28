# Nivel 2 - Profundidades Volcanicas

## Objetivo de la maqueta

Mostrar la funcionalidad principal del Nivel 2: ascenso vertical combinando levitacion con `Shift` y Wall Jump habilitado por la transformacion Araña de Mar con `Z`.

## Como abrir

1. Abrir Unity Hub.
2. Add project from disk.
3. Seleccionar:
   `C:\Users\ASUS\Documents\Codex\2026-05-26\files-mentioned-by-the-user-whatsapp\BoundByLight_Level2_Maqueta_Unity6000_3`
4. Abrir con Unity `6000.3.13f1`.
5. Abrir:
   `Assets/Scenes/Level2_Volcanes_WallJump.unity`

## Controles

- `A / D`: movimiento horizontal.
- `Espacio` o `W`: salto.
- `Shift`: levitacion, consume luz.
- `Z`: activa la forma Araña de Mar de Elias.
- Tocando una pared + `Espacio` o `W`: Wall Jump.
- `R`: reiniciar si hay Game Over.

## Que debe verse

- Fondo submarino profundo con textura azul.
- Capa media de volcanes y respiraderos hidrotermales.
- Ruta vertical de plataformas.
- Paredes laterales para Wall Jump.
- Geiseres activos como peligro ambiental.
- Orbes de luz para recargar energia.
- Nereo como babosa oscura.
- Elias transformado en Araña de Mar.

## Explicacion para clase

En el Nivel 2, el jugador ya conoce la levitacion del Nivel 1, pero ahora debe combinarla con una transformacion nueva: Araña de Mar. Elias se adhiere a superficies verticales, habilitando Wall Jump para que Nereo rebote entre paredes y ascienda sobre geiseres activos.

La tension del nivel viene de tres cosas:

- La ruta es vertical y exige precision.
- La luz de Elias se consume al levitar y al activar la Araña de Mar.
- Los geiseres obligan a calcular bien los saltos.

## Core loop del nivel

Recolectar luz -> activar Araña de Mar -> levitar hasta la pared -> hacer Wall Jump -> evitar geiser -> llegar a plataforma superior.

## Frase breve de presentacion

“Este nivel representa las profundidades volcanicas. El reto principal es ascender entre geiseres activos usando la ayuda de Elias transformado en Araña de Mar. La mecanica combina administracion de energia, lectura del entorno y ejecucion precisa de Wall Jump.”
