# Simulación de Boids y cazador

Proyecto de Unity **6000.3.17f1**, programado en C#. No requiere assets externos ni paquetes de terceros. Escena tridimensional con 8 agentes autónomos, un cazador, cuatro waypoints y un panel de observación.

## Abrir y ejecutar

1. En Unity Hub, usar **Add / Add project from disk** y seleccionar esta carpeta.
2. Abrir con **6000.3.17f1**.
3. Abrir `Assets/Scenes/Simulacion.unity` y pulsar **Play**.
4. Usar los botones del panel para pausar o cambiar la velocidad. Seleccionar un agente en la jerarquía y activar **Gizmos** para ver sus sensores en Scene.

La escena está incluida en Build Settings. El menú **Simulacion → Crear escena de demostracion** permite regenerarla; reemplaza la escena de demostración, por lo que hay que guardar los cambios propios en otra escena antes de usarlo.

## Arquitectura y consigna

| Requisito | Implementación |
| --- | --- |
| Mínimo 6 agentes | 8 instancias de `BoidAgent` |
| Autonomía sin líder ni órdenes globales | Cada Boid consulta `Physics.OverlapSphere` desde su propia posición; el panel solamente observa |
| Separation | Radio 1,8, fuerza inversamente proporcional a la distancia |
| Alignment y Cohesion | Radio 6; velocidad media y centro de vecinos vivos percibidos |
| Evade prioritario | Sensor de amenaza de radio 7, predicción de la posición del cazador; reemplaza flocking/Arrive y conserva separación para evitar choques |
| Evitar superposición | Separation, colliders sólidos y cuerpos rígidos con detección continua |
| Arrive e interacción | Frenado gradual, distancia de parada 1,35 y daño de 10 cada 0,7 segundos al objeto |
| Eliminación | Vida cero: velocidad cero y Rigidbody cinemático; permanece en el lugar |
| Recolección y reaparición | Desaparece al completar Gather; vuelve tras 5 segundos con vida completa en una posición aleatoria libre |
| FSM obligatoria | `HunterFSM`, estados Patrol, Attack y Gather; método privado de transición |
| Waypoints | Recorrido cíclico de cuatro puntos |
| Generación de intereses | El cazador intenta generar uno cada 5 segundos; máximo 5 activos |
| TBA y rangos | Campos públicos `TBA`, `RangeAttackRadius`, `MeleeAttackRadius` en el Inspector |
| Ataques | Cuerpo a cuerpo: persigue dentro de radio 2,4 hasta contacto 1,3; a distancia hasta radio 6; fuera de ambos persigue |
| Temporizador | Solo un ataque exitoso reinicia TBA y abandona Attack; perder el objetivo no reinicia TBA |
| Gather | Prioridad a cadáveres percibidos, aproximación y recolección durante 1,5 segundos; cancela si el objetivo no está disponible |
| Feedback | Estado, objetivo, sensores, temporizador, acciones, vida, colores y trazo de disparo |

Los sensores son omnidireccionales (360°) con radio limitado, adecuados a una arena abierta sin obstáculos interiores. Los límites son paredes físicas. Los cadáveres permanecen como obstáculos hasta ser recolectados. Los disparos son instantáneos con un trazo visual; no requieren proyectiles físicos.

`AgentMotor` comparte movimiento y búsqueda de posiciones libres para aparición. Esta búsqueda no controla las decisiones de los agentes. `SimulationHUD` conserva referencias para mostrar datos, sin coordinar el grupo. La FSM prioriza Gather por encima de Attack al percibir un cadáver.

## Parámetros

Seleccionar `Cazador` o un `Boid` en la jerarquía para modificar los campos del Inspector. La vida inicial de un Boid es 60; un golpe causa 60 y un disparo 30. Cada interés tiene 45 de vida. Mantener Separation por debajo del radio de flocking y el radio de contacto por encima de la suma de los radios físicos del cazador y del Boid.

## Verificación reproducible

Con el editor cerrado, ejecutar en PowerShell desde esta carpeta:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.17f1\Editor\Unity.exe' -batchmode -nographics -projectPath "$PWD" -executeMethod ProjectBuilder.RunSmoke -smokeTest -logFile "$PWD\smoke-test.log"
```

La prueba entra en Play Mode, verifica las condiciones de la FSM, eliminación/reaparición y ejecuta 90 segundos simulados. Finaliza con código 0 y `SMOKE_TEST_OK` si pasa. El componente de pruebas se excluye de los builds y solamente se instala al utilizar `-smokeTest`.

Para generar la escena desde la línea de comandos, usar `-executeMethod ProjectBuilder.BuildScene -quit` en lugar de `-executeMethod ProjectBuilder.RunSmoke -smokeTest`.
