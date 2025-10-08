# AR Gender Prediction 🎭

Ini adalah proyek Ujian Tengah Semester (UTS) untuk mata kuliah Augmented Reality/Virtual Reality. Aplikasi ini menggunakan **AR Foundation** untuk mendeteksi wajah pengguna secara *real-time*. Wajah yang terdeteksi kemudian dianalisis oleh sebuah model AI di *backend* untuk memprediksi gender dan secara dinamis menampilkan aksesori virtual (prefab) yang sesuai.



---
## ✨ Fitur Utama

* **Deteksi Wajah Real-Time:** Menggunakan `ARFaceManager` dari AR Foundation untuk melacak wajah melalui kamera depan.
* **Integrasi AI:** Mengirim gambar wajah yang terdeteksi ke *backend* untuk dianalisis oleh model *deep learning*.
* **Visualisasi AR Dinamis:** Menampilkan prefab yang berbeda (misalnya, kumis atau kacamata) yang menempel mengikuti wajah pengguna berdasarkan hasil prediksi.
* **UI Informatif:** Memberikan umpan balik kepada pengguna mengenai status deteksi dan hasil prediksi.

---
## 🛠️ Teknologi yang Digunakan

* **Frontend (Aplikasi AR):**
    * Unity
    * AR Foundation
* **Backend (Server AI):**
    * Python
    * FastAPI
    * Gunicorn & Uvicorn

---
## 📋 Prasyarat

Sebelum memulai, pastikan perangkat Kamu telah terinstal:

* **Unity Hub**
* **Unity**
* **Modul Android Build Support:** Pastikan ditambahkan saat instalasi Unity (termasuk **OpenJDK** dan **Android SDK & NDK Tools**).
* **Git** untuk kloning repositori.
* **Perangkat Android** yang mendukung ARCore untuk pengujian.

---
## 🚀 Panduan Menjalankan Proyek Unity

#### 1. Kloning Repositori

Buka terminal atau Git Bash, lalu jalankan perintah berikut untuk mengunduh proyek:
```bash
git clone [https://github.com/FajarRamadhanBBX/AR-for-gender-prediction.git](https://github.com/FajarRamadhanBBX/AR-for-gender-prediction.git)
```

#### 2. Buka Proyek di Unity

1.  Buka **Unity Hub**.
2.  Klik tombol **"Open"**.
3.  Navigasi dan pilih folder proyek yang baru saja Anda kloning, lalu klik **"Open"**.
4.  Unity akan mulai mengimpor semua aset. Proses ini mungkin memakan waktu beberapa menit.

#### 3. Verifikasi Paket AR Foundation

Proyek ini seharusnya sudah menyertakan AR Foundation. Untuk memastikannya:
1.  Buka **Window > Package Manager**.
2.  Pastikan paket **"AR Foundation"**, **"ARCore XR Plugin"**, dan **"ARKit XR Plugin"** sudah ada di dalam daftar. Jika belum, cari dan instal dari **"Unity Registry"**.

#### 4. Atur Platform Build ke Android

1.  Buka **File > Build Settings...**
2.  Pilih **Android** dari daftar platform.
3.  Jika belum menjadi platform aktif, klik tombol **"Switch Platform"**. Tunggu hingga prosesnya selesai.

#### 5. Konfigurasi Backend

Sebelum menjalankan aplikasi, pastikan *backend* sudah berjalan (baik secara lokal maupun di server cloud) dan Anda sudah memasukkan alamat IP yang benar. Panduan untuk menjalankannya dapat melihat instruksi di paling bawah.
1.  Buka *scene* utama (misalnya `SampleScene`).
2.  Pilih GameObject **`FacePrefab`**.
3.  Di Inspector, cari skrip **`Face Accessory Manager`**.
4.  Ubah *field* **`Api URL`** dengan alamat IP *backend* yang sudah kamu jalankan (misal: `http://192.168.1.7:8000/predict`).

#### 6. Build dan Jalankan

1.  Hubungkan perangkat Android Anda ke komputer.
2.  Di **Build Settings**, klik **"Build"**.
3.  Pilih lokasi untuk menyimpan file `.apk` Anda.
4.  Pindahkan aplikasi ke smartphone, dan install aplikasi.

---
## ☁️ Panduan Menjalankan Backend

Untuk instruksi cara menjalankan server *backend* (baik secara lokal maupun di AWS EC2), silakan melihat ke dokumentasi `README.md` di [repositori backend](https://github.com/FajarRamadhanBBX/backend-for-AR-gender-detection).
