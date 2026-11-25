# Galaxy Explorer engine migration

**New target engine: Godot 4.** The migration is being developed on the `godot4-migration` branch so the previous Unity source remains recoverable during the port.

Open the Godot project by importing `godot/project.godot` in Godot 4.x. The active migration foundation includes a procedural Solar System, orbital motion, a starfield, free camera controls, local discoveries, and local camera-state persistence.

The Unity implementation is legacy/reference material during migration. It is not evidence of a working Godot build. The Godot project still needs parsing and runtime verification in the Godot editor, followed by Android export-template setup, APK export, and real-device testing.
