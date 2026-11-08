# ROUGHTALE8 — AAA Cinematic Vertical Slice (Handoff untuk EV)

> Status: **prototype jalan, tanpa error console.** File asli `web-prototype/index.html` **tidak disentuh sama sekali.**

## 1. File & cara menjalankan

| Item | Path |
|---|---|
| Build AAA (baru) | `index_aaa.html` — **di root `GAME ROUGHTALE8`, gampang dicari** |
| Build lama (EV) | `web-prototype/index.html` — **jangan diedit dari sisi AAA** |
| Asset baru | `web-prototype/assets/aaa/` |
| Asset lama | `web-prototype/assets/` |
| Launcher | `Run_ROUGHTALE8_AAA.bat` (root project) |

Jalankan lewat **HTTP server**, bukan `file://` (ES modules + fetch GLB/HDR diblokir CORS):

```bat
cd "C:\Users\Faari\Documents\GAME ROUGHTALE8"
python -m http.server 8090 --bind 127.0.0.1
```

Buka `http://127.0.0.1:8090/index_aaa.html`.
Atau cukup dobel-klik `Run_ROUGHTALE8_AAA.bat`.

> Catatan: `index_aaa.html` ada di root, jadi semua path asset di dalamnya memakai prefix `web-prototype/` (mis. `web-prototype/assets/aaa/models/Soldier.glb`). Jangan dipindah lagi tanpa menyesuaikan path.

## 2. Perubahan besar

### Renderer & pipeline
- Three.js **r128 global → r170 ES modules** (importmap + **vendor lokal**, bukan CDN).
  - Library disimpan di `web-prototype/assets/aaa/vendor/three/` (`three.module.js` + 13 file addon).
  - Alasan: unpkg CDN butuh **~3.2 detik** untuk `three.module.js` saat cache dingin — ini penyebab utama loading lama. Sekarang 0 request keluar.
- `ACESFilmic` → **AgX tone mapping** (fallback ACES), exposure 1.08, `outputColorSpace = SRGB`.
- Post-processing `EffectComposer`:
  - `UnrealBloomPass` (strength 0.28, radius 0.70, threshold 0.90)
  - `OutputPass`
  - Custom **grade shader**: teal shadows / warm highlights, S-curve contrast, vignette, film grain, chromatic aberration.
- `renderer.toneMappingExposure`, pixel ratio tetap dibatasi 1.5.

### Lighting
- **HDRI IBL**: `spruit_sunrise_1k.hdr` → `scene.environment`, `environmentIntensity 0.75`.
- Sun directional 0xffd3a3 @ 2.4, shadow map **2048**, radius 3, bias -0.0004, frustum 62.
- Hemi 0.28, cool rim 0.35, warm camera-side fill 1.1.
- Lampu legacy (point/spot) dikali **22** sekali di startup karena r155+ pakai physical units. Flashlight di-set 55, alley light 70.
- Flashlight **default OFF** (tekan `F`), termasuk beam mesh-nya.
- Sky: gradient shader dome (dingin di atas, hangat di horizon) + sun sprite additive.

### Material & tekstur
- 4 set PBR Poly Haven (CC0) 1K: **asphalt_02, concrete_wall_008, rusty_metal_02, concrete_floor_damaged_01** → diffuse + normal + roughness (AO diunduh, belum dipakai).
- `MeshStandardMaterial` untuk jalan/beton/rust/sidewalk dengan anisotropy maksimum.
- Cat mobil jadi `MeshPhysicalMaterial` + **clearcoat** (showroom sheen).
- Puddle jadi `MeshPhysicalMaterial` + alpha map radial (tepi lembut), opacity 0.55.
- Emissive (visor, shard, headlight) diturunkan agar bloom tidak blow-out.

### Karakter hero (Eidon)
- Model **`Soldier.glb`** (rig Mixamo, 1.82 m, rotasi Y = π) menggantikan tubuh prosedural.
- State machine `AnimationMixer`: **Idle / Walk / Run**, crossfade 0.22 s, timeScale beda untuk walk vs sprint. Saat udara pakai `Run`.
- Bagian prosedural lama (`eidonLegacy`) di-hide saat GLB selesai load; **fallback prosedural tetap ada** kalau GLB gagal.
- Tanpa coat tambahan: siluet memakai armor asli `Soldier.glb`. Coat/belt prosedural yang sempat ditambahkan sudah dihapus (terlihat seperti rok/ember dari kamera belakang).
- Semua mesh GLB & NPC: `castShadow` + `receiveShadow`.
- Warna body disetel ke 0x55606d, roughness 0.78, envMapIntensity 0.55 supaya tidak putih pucat.

### Dunia
- Partikel debu: 420 titik, soft radial texture, additive.
- NPC prosedural tetap (belum diganti GLB).
- BGM `preload="none"` (4.7 MB baru diunduh saat user klik masuk), volume **0.15** supaya hanya jadi latar.

## 3. Yang dipertahankan (regression check)
- 3 mode kamera: **RE4 over-the-shoulder → Tactical third-person → First-person visor** (tombol `V` / UI). Sudah dites, label berubah benar.
- Jump physics, ground height (plaza/step trotoar), shard collection, dialog NPC, radar, HUD, audio.
- `F` toggle flashlight: dites, spotlight `visible false → true`.
- Console: **0 error** saat load dan setelah masuk dunia.

## 4. Performa & Optimasi 60 FPS (Updated oleh EV)
- Load awal (HTTP lokal): **~1.0 detik** (`loadEventEnd`), dari sebelumnya ~3–5 detik karena CDN.
- Baseline sebelumnya: **~53 FPS** (rAF 2 detik, viewport 1600 px, shadow map 2048, bloom 1.0×, pixelRatio 1.5, PointLight fire barrel castShadow aktif).
- **Optimasi yang sudah diterapkan oleh EV di `index_aaa.html`:**
  1. `sunLight.shadow.mapSize` diturunkan dari `2048×2048` ke **`1536×1536`**.
  2. `UnrealBloomPass` dirender pada resolusi **`0.7×`** (`Math.floor(innerWidth * 0.7) × Math.floor(innerHeight * 0.7)`), termasuk saat `window.resize`.
  3. `renderer.setPixelRatio` dibatasi maksimal **`1.25`** (dari `1.5`).
  4. Menonaktifkan cube-map shadow pada 3 `fireLight` (`fireLight.castShadow = false`), menghemat 18 shadow pass per frame.
  5. Menggabungkan (*merge*) geometri pilaster/ledge beton bangunan per gedung via `BufferGeometryUtils.mergeGeometries` agar penambahan detail arsitektur 3D tidak membebani *draw calls*.
- **Hasil Pasca-Optimasi:** Mencapai target **60 FPS stabil** (vsync 60.0 FPS).

## 5. Known issues / next steps (Status Penyelesaian EV)
1. ✅ **Bangunan latar sudah di-upgrade:** Facade modular 3D (5 pilaster vertikal per gedung + cornice/spandrel tiap lantai), kaca *curtain-wall* reflektif yang menjorok ke dalam (*recessed parallax*), kotak jendela *emissive* (warm amber & cyan KAMI) dengan *emissiveMap* + efek tirai (*blinds*), *rooftop setback*, menara komunikasi ber-beacon merah, serta barisan gedung lapis kedua (*mid-background megastructures* di `x = ±46..48`).
2. ✅ **3 NPC Utama sudah memakai `Xbot.glb`:** `DR. LIRA` (`idle`), `KAEL` (`idle` + pedang punggung), dan `PATROLMAN CROW` (`walk` saat patroli, otomatis *crossfade* ke `idle` saat dialog dibuka), masing-masing dengan `AnimationMixer` dan palet material tersendiri. 7 NPC lainnya tetap prosedural; dialog & radar 100% berfungsi.
3. ✅ **Siluet Coat Eidon diperbaiki (`coatTailAAA`):** Dirancang ulang sebagai **Split-Back Tactical Tailcoat** 2 panel (kiri & kanan) dengan bagian depan terbuka penuh, belahan *V-split* tegas di punggung belakang, *taper* pinggang oval yang ramping (`coatBelt` & *peplum yoke*), serta *flutter* independen kiri-kanan saat berlari (bukan silinder rok/ember).
4. ✅ **Target 60 FPS tercapai.**
5. Belum ada musuh/AI; slice ini fokus visual.
6. `Run_ROUGHTALE8_Web.bat` lama tetap membuka `web-prototype/index.html`; tidak diubah.

## 7. Debug handle
```js
window.__ROUGHTALE8 = { scene, camera, renderer, composer, playerGroup, npcs, memoryShards, landmarks }
```
Dipakai untuk tuning lighting/material langsung dari console.

## 8. Aturan koordinasi
- EV lanjut di `web-prototype/index.html`; build AAA di `index_aaa.html` (root).
- Kalau nanti digabung: ambil blok renderer/post-processing/lighting dari `index_aaa.html`, lalu port fitur gameplay EV satu per satu. Jangan copy seluruh file.
- Asset di `web-prototype/assets/aaa/` boleh dipakai kedua build.

---

## Update OP - 6 Nov 2026 (lanjutan dari handoff EV)

> Catatan: web-prototype/index.html tetap tidak disentuh. Semua perubahan di index_aaa.html.

### 1. Tubuh Eidon = rig Soldier.glb (kualitas armor terbaik yang kita punya)
- Hero memakai web-prototype/assets/aaa/models/Soldier.glb (2 skinned mesh + animasi Idle / Walk / Run).
- Material di-retheme: body graphite (0x8b95a3, roughness 0.58, metalness 0.32, env 1.05); visor cyan emissive 0x38bdf8.
- Ditambah heroKeyLight (PointLight 2.6, range 4.5) sebagai fill agar detail anatomi terbaca di gang gelap.
- Rig manusia prosedural EV tetap ada sebagai fallback jika GLB gagal dimuat (eidonRig.root.visible = !eidonModel).
- State machine animasi: setEidonAction('Idle'/'Walk'/'Run') + eidonMixer.update(delta) di loop utama.
- Konsekuensi: Eidon kembali tampil berzirah (bukan hoodie). Kalau mau balik ke wujud manusia, cukup jangan load GLB / set eidonModel.visible = false.

### 2. Ambient Occlusion (aoMap)
- loadTextureSet() sekarang juga memuat <nama>_ao.jpg dengan repeat yang sama.
- Dipasang di: aspal (0.55), trotoar (0.5), beton/pier (0.6), brutalist (0.38), rust metal (0.45).
- Three r170 memakai 	exture.channel = 0 (default), jadi AO memakai UV utama - tidak perlu uv1.

### 3. Radar minimap
- Layout jalan, trotoar, plaza, zebra cross, lane dashes, dan monumen di-bake sekali ke canvas offscreen (adarBase).
- Tiap frame hanya drawImage yang di-translate mengikuti posisi pemain + clip lingkaran radar (biaya hampir nol).
- Blip lama (shard / landmark / NPC / pemain) tetap; ditambah blip merah berdenyut untuk KAMI Anomaly.

### 4. KAMI Anomaly (musuh pertama)
- Entitas baru di gang maintenance (x -32, z 22..44), memakai createHumanCharacterRig bertema hitam + visor/core merah emissive.
- State machine: patrol -> alert -> chase -> (contact) -> patrol.
- Deteksi: radius 18 m saat senter menyala, 11 m saat gelap; FOV +-0.95 rad; butuh p.y < 6.
- Contact sekarang NON-LETHAL: pemain didorong 3.2 m, layar berkedip merah, lalu anomaly cooldown 7 detik dan kembali patroli.
- HUD: badge #anomaly-alert + overlay #anomaly-flash.
- Belum ada sistem HP/damage. Menunggu keputusan desain.

### 5. Performa
- Ditambah adaptive resolution: jika rata-rata < 50 FPS selama 2.5 detik, pixel ratio turun 0.125 sampai minimum 1.0 (tidak pernah menurunkan di bawah itu).
- Catatan: pengukuran FPS di sesi OP tidak stabil (tab browser throttled saat tidak visible). Mohon ukur ulang di mesin EV.

### 6. Debug handle
window.__ROUGHTALE8 sekarang juga berisi: eidonRig, getter eidonModel/eidonMixer, kamiAnomaly, drawRadar, adarCanvas, daptivePixelRatio.



---

## Update EV - 6 Nov 2026 (Penyempurnaan Lanjutan KAMI Anomaly & Imersi)

1. **Eidon Default Preserved (Human Rig):**
   - Sesuai kode index_aaa.html, Eidon default tetap menggunakan Human Character Rig ber-hoodie tanpa zirah sesuai visi Faari. Mode zirah Soldier.glb tetap tersedia sebagai opt-in via parameter URL `?hero=armor` jika ingin membandingkan.

2. **KAMI Anomaly Full Locomotion & Horror Stalker Animation:**
   - Menambahkan siklus animasi prosedural penuh pada rig KAMI Anomaly (bukan lagi patung kaku yang meluncur):
     - **Patrol Mode:** Langkah tegap kaku beritme lambat (slow menacing stride) dengan ayunan lengan minim dan kepala mengamati sekitar.
     - **Chase Mode:** Postur tubuh membungkuk agresif ke depan (hunched predatory lunge), ayunan kaki cepat, dan kedua lengan menjangkau ke depan dengan siku tertekuk seolah hendak menerkam Eidon.
     - **Alert Mode:** Diam tegap mengintai dengan kepala mengunci langsung ke arah posisi Eidon.

3. **Audio Synthesis SFX untuk KAMI Anomaly:**
   - `playAnomalyAlertSound()`: Disonansi frekuensi rendah (sawtooth 140Hz -> 540Hz) dan nada tinggi menyeramkan saat anomali mendeteksi Eidon.
   - `playAnomalyHitSound()`: Dentuman frekuensi rendah terdistorsi + letupan glitch white noise saat anomali menabrak dan mendesinkronisasi visor Eidon.

4. **Visceral Impact Feedback & HUD Condition System:**
   - **Lensa Kamera & Shading:** Lonjakan chromatic aberration pada `gradePass.uniforms.aberration` naik ke `0.018` seketika saat tabrakan, lalu pulih secara halus (decay) ke baseline sinematik `0.0016`.
   - **Camera Shake:** Efek getaran guncangan kamera fisik (`cameraShake = 0.42`) yang meluruh secara eksponensial.
   - **HUD Condition Badge:** Indikator kondisi berubah secara dinamis:
     - Seketika: `CONDITION: ⚠ DESYNC // CORRUPTED` (merah menyala dengan pendaran neon merah).
     - Setelah 3.5 detik: `CONDITION: RE-STABILIZING...` (amber).
     - Setelah 6.8 detik: Kembali ke `CONDITION: FINE` (hijau normal).

5. **Orbiting Corruption Motes:**
   - Menambahkan 5 pecahan tetrahedron hitam-merah berputar mengitari tubuh KAMI Anomaly secara acak untuk mempertegas aura kutukan supernatural Siklus 8.
