# ROUGHTALE8 — AAA Cinematic Vertical Slice (Handoff dari EV untuk OP)

> **Status Saat Ini:**  
> - **Game Berjalan Lancar (0 Error Console).**  
> - **Start Screen & Button Masuk:** 100% responsif (*click / pointerdown / touchstart / inline fail-safe*).  
> - **Model Karakter:** Eidon kini tampil sebagai **wujud manusia asli** (bukan prajurit berzirah *Soldier.glb*), lengkap dengan 10 NPC berbusana dan berkarakter unik.  
> - **Kendaraan & Tabrakan:** Mobil tidak lagi amblas ke tanah (*ground clearance suspension offset*), dilengkapi bodi modern aerodinamis dan *collision hulls* sehingga tidak tembus karakter.  
> - **Lingkungan:** Ditambahkan partikel uap manhole (*steam vents*), billboard neon hologram bercahaya, barikade kawat duri, dan cincin plasma monumen KAMI.  
> - **Performa:** 60 FPS stabil pada viewport render canvas.

---

## 1. Peta File & Akses Proyek

| File / Folder | Peran & Keterangan |
|---|---|
| `index_aaa.html` | File kerja utama AAA di root `GAME ROUGHTALE8`. Semua aset menggunakan path `web-prototype/...`. |
| `serve_aaa.py` | Dedicated fast HTTP server di `127.0.0.1:8090` dengan auto-launch browser dan handling port collision aman. |
| `Run_ROUGHTALE8_AAA.bat` | Launcher satu-klik bersih; menggunakan `pythonw` (atau background minified) sehingga **tidak membuka 2 jendela command prompt**. |
| `web-prototype/index.html` | Prototype lama EV — **tetap utuh dan tidak disentuh**. |
| `web-prototype/assets/aaa/` | Aset tekstur Poly Haven PBR 1K, HDRI `spruit_sunrise_1k.hdr`, dan vendor Three.js r170 lokal. |

### Cara Menjalankan
Cukup dobel-klik `Run_ROUGHTALE8_AAA.bat`, atau jalankan secara manual:
```bash
python serve_aaa.py
```
Lalu akses di browser: **`http://127.0.0.1:8090/index_aaa.html`**

---

## 2. Rangkuman Detail Pekerjaan yang Diselesaikan EV

### A. Karakter Utama: Wujud Manusia Asli Eidon
Berdasarkan arahan Faari dan referensi konsep visual (`web-prototype/assets/characters.png`), zirah militer `Soldier.glb` telah digantikan oleh **Human Character Rig** prosedural berkualitas tinggi (`createHumanCharacterRig`):
1. **Anatomi & Proporsi Manusiawi:** Proporsi tubuh pria atletis 1.82 m dengan kepala bertekstur warna kulit alami, fitur mata berfokus, dan rambut hitam berantakan (*tousled hair styling*).
2. **Outfit Signature Eidon:**
   - Jaket hoodie kasual abu-abu gelap dengan tudung bertengger di bahu dan aksen tali kerah (*hood drawstrings*).
   - Celana kargo taktis tahan cuaca dengan saku samping (*side cargo pouches*) dan sabuk kulit.
   - Sepatu boot penjelajah berpelindung tapak kokoh.
   - Sarung tangan *fingerless tactical gloves* berbahan kulit sintetis.
3. **Equipment:** Ransel kanvas cokelat petualang (*canvas haversack rucksack*) dengan strap dada melintang, pengikat kantung tidur bawah, dan gesper kuningan.
4. **Animasi Siklus Gerak Prosedural:**
   - *Idle Breathing*: Gerakan naik-turun dada dan bahu halus (frekuensi ~1.4 rad/s).
   - *Dynamic Locomotion*: Ayunan lengan dan langkah kaki tersinkronisasi frekuensi lari/jalan, *torso pitch lean* saat sprint, serta rotasi pinggul dinamis.
   - *Jump & Air State*: Kaki tertekuk dan lengan terangkat saat mengudara.

---

### B. Visual Archetypes NPC (10 Karakter Unik)
Seluruh NPC tidak lagi memakai silinder primitif. Setiap NPC kini memiliki rig tubuh manusia proporsional dengan busana, warna, dan atribut visual spesifik yang langsung mencerminkan lore mereka:
1. **Dr. Lira** (`x: 0, z: -10`): Jas lab dokter putih panjang, celana formal biru dongker, stetoskop melingkar di leher, kacamata baca tipis, dan clipboard rekam medis pasien di tangan kiri.
2. **Kael** (`x: 10, z: -2`): Trenchcoat hitam panjang bergaya modern, kerah tinggi, aksen ikat pinggang kulit gelap, dan sepasang sarung pedang/katana bersilang di punggung (*dual back-scabbards*).
3. **Nara** (`x: -8, z: 8`): Jaket bomber penerbang kulit cokelat tua dengan kerah bulu (*aviator shearling jacket*), kacamata goggle pilot di dahi, sarung tangan kulit, dan pouch utilitas mekanik.
4. **Reza** (`x: 6, z: 12`): Jubah pengintai gurun (*desert scout cloak*) bertudung tipis, celana kamuflase debu, kacamata visor pelindung debu pasir, dan busur *recurve* taktis tersampir di punggung.
5. **Vance (Tetua Distrik)** (`x: -16, z: -8`): Mantel wol tebal kancing ganda, syal bergaris panjang di leher, kupluk rajut musim dingin (*winter beanie*), jenggot kelabu tipis, serta tangan memegang cangkir timah kopi yang mengepulkan uap panas.
6. **Nurse Maya** (`x: -18, z: 4`): Pakaian perawat rumah sakit warna hijau toska (*teal scrubs*), celemek medis putih (*medical apron*), masker bedah ditarik di dagu, dan koper P3K darurat dengan logo palang merah (*red-cross medkit*).
7. **Tariq (Scavenger)** (`x: 18, z: -12`): Helm proyek keselamatan warna kuning cerah dengan lampu kepala (*LED headlamp*) menyala, rompi reflektif oranye terang, dan tas rongsokan salvage raksasa yang membawa potongan pipa besi bekas.
8. **Officer Briggs** (`x: 14, z: 16`): Helm antihuru-hara SWAT lengkap dengan visor pelindung bening, rompi pelat balistik tebal (*heavy tactical plate*), lencana kepolisian di dada, dan tongkat pentungan listrik di sabuk.
9. **Patrolman Crow** (`x: -4, z: -25`): Topi dinas polisi (*peaked service cap*), rompi polisi lalu lintas warna hijau neon dengan strip reflektif perak, dan tongkat lalu lintas LED merah menyala (*traffic baton*) saat berpatroli.
10. **Sentry Elena** (`x: 22, z: 2`): Baret militer merah marun, headset komunikasi radio dengan mikrofon tangkai, seragam tempur taktikal zaitun, dan senapan runduk jarak jauh (*anti-materiel sniper rifle*) di punggung.

---

### C. Kendaraan & Sistem Fisika Tabrakan
1. **Model Bodi Baru (`createSculptedVehicle`):**
   - Geometri bodi mobil aerodinamis ekstrusi multi-layer (Sedan & SUV).
   - Detail bodi: *Power-dome hood bulge*, sirip antena atap (*shark-fin*), spoiler sport belakang (*rear wing spoiler*), kaca film gelap (*smoked glass*), spion samping ganda, jok interior, dan lingkar kemudi setir.
   - Varian warna: Sedan Biru Tengah Malam, Sedan Crimson Sport, SUV Militer, dan Mobil Polisi Distrik dengan lampu strobo sirine aktif.
2. **Solusi Amblas ke Tanah:**
   - Posisi mobil sekarang dihitung langsung terhadap ketinggian jalan (`getSurfaceElevationAt(x, z)`) ditambah toleransi *ground clearance* suspensi ban sebesar `+0.14 m`. Roda menapak sempurna di permukaan aspal tanpa pernah tenggelam.
3. **Collision Box (Anti-Tembus):**
   - Setiap mobil dan barikade beton kini otomatis mendaftarkan volume fisiknya ke dalam array global `colliders` sebagai AABB (*Axis-Aligned Bounding Box*).
   - Sistem pergerakan Eidon memverifikasi batas tabrakan ini setiap frame sehingga karakter tidak bisa lagi berjalan menembus bodi mobil atau tiang lampu jalan.

---

### D. Lingkungan & Suasana Sinematik (Atmosphere)
1. **Steam Vents (Uap Manhole):** Menambahkan sistem partikel uap dinamis yang mengepul perlahan dari lubang got/manhole jalan raya dengan variasi acak kecepatan dan transparansi lembut.
2. **Barikade Kawat Duri:** Pagar kawat duri melingkar (*coiled barbed wire*) berbahan logam reflektif tinggi di atas beton pembatas jalan militer.
3. **Neon Hologram Glitch Signs:** Papan nama neon cyberpunk bertuliskan *"DISTRICT 8"*, *"KAMI WATCHES"*, dan *"MEM-BANK"* yang memancarkan pendaran cahaya (*bloom*) dan berkedip (*subtle flicker*).
4. **Cincin Plasma Monumen KAMI:** Di puncak obelisk monumen pusat, kini terdapat cincin partikel plasma berputar dengan pulsasi emisi biru langit (0x38bdf8).

---

### E. Bug Kritis yang Diselesaikan
1. **Tombol Start / Masuk Dunia Tidak Merespons:**
   - *Akar Masalah:* Adanya `ReferenceError` akibat material `matBarbedWire` & `matHoloBlue` dipanggil sebelum baris deklarasinya saat file diedit sebelumnya.
   - *Solusi:* Seluruh variabel material telah dirapikan ke Section 3 (baris 1.200) sebelum semua geometri dipanggil. Tombol `#btn-enter-world` kini memiliki multi-listener (`click`, `pointerdown`, `touchstart`) serta inline fallback `onclick`.
2. **Tombol Tutup Dialog `[E]` & `[Escape]`:**
   - *Akar Masalah:* Ketika dialog shard dibuka tanpa mengunci pergerakan (`lockMovement = false`), flag `isDialogOpen` bernilai `false`, menyebabkan event keydown `[E]` mengabaikan perintah penutupan.
   - *Solusi:* Pengecekan penutupan kini langsung membaca status kelas CSS `dialogModal.classList.contains('open')`.
3. **Pengambilan Shard Memori Cepat (Zero Latency):**
   - Logika pengambilan shard disederhanakan tanpa ada request asinkron yang menghambat UI; sound effect synthesizer Web Audio API dijalankan instan.
4. **Dual Terminal pada Launcher:**
   - `Run_ROUGHTALE8_AAA.bat` kini mendeteksi dan mengeksekusi `pythonw` (mode tanpa jendela console), sehingga pemain tidak lagi melihat 2 jendela terminal hitam saat membuka game.

---

## 3. Handle Debug & Tuning Runtime

Untuk memeriksa state scene, lighting, dan collider dari browser Developer Tools:
```javascript
// Akses handle global
window.__ROUGHTALE8

// Contoh: cek collider yang aktif
console.log(window.__ROUGHTALE8.colliders);

// Contoh: cek daftar NPC & karakter rig
console.log(window.__ROUGHTALE8.npcs);

// Contoh: panggil fungsi masuk dunia secara manual
window.__enterWorld();
```

---

## 4. Rekomendasi Langkah Selanjutnya untuk OP

Jika OP ingin melanjutkan pengembangan berikutnya, berikut beberapa rekomendasi area yang siap ditingkatkan:
1. **Custom Mesh Import (Opsional):** Jika ingin mengimpor model GLB rigged buatan Blender untuk Eidon atau NPC, slot fungsi `createHumanCharacterRig` dapat di-switch dengan `GLTFLoader` memakai state machine `AnimationMixer` yang sudah siap di pipeline.
2. **Entitas Musuh / KAMI Anomaly:** Slice saat ini difokuskan pada *narrative exploration & atmosphere*. Logika patroli NPC `Patrolman Crow` bisa diadopsi untuk menciptakan entitas musuh pengintai KAMI dengan radius deteksi dan stealth alert.
3. **Pemanfaatan Texture AO:** 4 set PBR Poly Haven di `web-prototype/assets/aaa/textures/` sudah memiliki file tekstur AO (*ambient occlusion*). Bisa dimasukkan ke properti `aoMap` material jalan dan gedung untuk menambah kedalaman kontras bayangan di sela-sela arsitektur.
4. **Mini-map Map Canvas:** Radar pojok kiri bawah saat ini menampilkan blip 2D relative angle. Bisa dipercantik dengan siluet layout jalan mini-map berbasis 2D canvas drawing.

---
*Dokumentasi ini disiapkan oleh EV untuk kolaborasi berkelanjutan bersama OP dan Faari.*
