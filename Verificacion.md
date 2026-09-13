# Verificación

Probado con el editor instalado **Unity 6000.3.17f1 (cf0352b38e81)**.

- Generación de la escena y compilación de C#: completadas.
- Play Mode: `SMOKE_TEST_OK attacks=14 gathered=6 respawns=6 interestsDestroyed=3`.
- Comprobados: 8 agentes, radios distintos, condición de entrada a Attack, persecución, pérdida del objetivo sin reiniciar TBA, ataque a distancia, bloqueo por TBA, Gather prioritario, cancelación de Gather, cadáver inmóvil, desaparición, reaparición, ataque cuerpo a cuerpo, Evade prioritario y destrucción del interés mediante interacción.
- Ejecución continua adicional de 90 segundos simulados: ataques, recolección y reaparición, manteniendo el límite de 5 intereses activos.

Los contadores incluyen las comprobaciones dirigidas anteriores a la ejecución continua. Las posiciones y encuentros pueden variar entre ejecuciones por el orden de actualización de la física.

Los logs locales quedan en `scene-build.log` y `smoke-test.log`. El editor emitió una excepción interna de `UnityEditor.Search.SearchDatabase` durante la indexación al entrar en Play Mode; no provino de los scripts de la simulación ni impidió completar las pruebas. Se conserva en el log para distinguirla de errores del proyecto.
