# 🚀 FastBmp: High-Performance Image Processing in C#

This educational project implements an ultra-fast **Client-Server** system designed to apply grayscale filters to heavy BMP images. The primary objective was to minimize RAM footprint and maximize CPU throughput by leveraging low-level .NET programming techniques.

## 🛠️ Technical Challenges Overcome

### 1. "Zero-Copy" Memory Management
* **ArrayPool<T>:** Integrated `ArrayPool<byte>.Shared` to recycle 100MB buffers. This mitigates Garbage Collector pressure and prevents performance spikes associated with the Large Object Heap (LOH).
* **In-Place Processing:** Images are modified directly within the reception buffer. No new array allocations occur during the filtering stage, significantly reducing latency.

### 2. Unsafe Programming & Pointers
* **Pointer Manipulation (`byte*`):** Direct pixel access via memory addresses to bypass .NET's standard bounds-checking overhead.
* **Structure Mapping:** Utilized `MemoryMarshal.AsRef` to map raw bytes directly onto BMP headers (`BITMAPFILEHEADER`, `BITMAPINFOHEADER`), eliminating the need for expensive bitwise conversions.

### 3. Parallelism & CPU Optimization
* **Parallel.For:** Workload is partitioned by image rows to exploit all available CPU cores.
* **Memory Pinning:** Employed the `fixed` statement combined with `nint` (Native Integer) captures to ensure the Garbage Collector does not relocate data while worker threads are active.

### 4. Low-Level Networking (Sockets)
* **Custom Protocol:** Implemented a binary "Size-Prefix" protocol (4-byte length header + payload).
* **Streaming I/O:** Data transfer via `NetworkStream` using 64KB chunks, allowing the processing of multi-gigabyte files with a constant, minimal RAM footprint.
* **Concurrency Control:** Leveraged `SemaphoreSlim` on the server-side to throttle concurrent clients and protect system resources from exhaustion.

---

## 📈 Performance Results

* **Processing Time (CPU):** ~0.XXs for a 100MB image (hardware dependent).
* **RAM Consumption:** Highly stable even during stress tests, thanks to aggressive buffer recycling.
* **Scalability:** Capable of processing files larger than the available physical RAM through disk-to-network streaming.

---

## 📂 Project Structure

* **FastBmpServer:** Asynchronous server handling buffer pooling and image processing.
* **FastBmpClient:** High-performance client for sending and receiving binary data via sockets.
* **BmpLogic:** Shared library containing binary structures and `unsafe` algorithms.

---

## 💻 How to Run

1.  **Start the server:** ```bash
    dotnet run --project FastBmpServer
    ```
2.  **In a separate terminal, run the client:** ```bash
    dotnet run --project FastBmpClient
    ```
3.  **Monitor performance:** Use Windows Task Manager or `dotnet-counters` to observe RAM stability.

---

> **Educational Note:** This project demonstrates that C# is not "slow" or "too high-level." By using the right tools (`Span<T>`, `Pointers`, `MemoryMarshal`), one can achieve performance metrics comparable to C++.

---

### 💡 Pro-Tip: Validating Memory Stability
To prove the efficiency of the **SemaphoreSlim** and **ArrayPool** implementation, perform a stress test:
1. Run a loop of 100 client requests for a 100MB file.
2. Observe that the Server's RAM usage (Working Set) stabilizes quickly (e.g., around 600-800MB) and does not grow linearly with the number of requests.
3. This demonstrates successful **buffer reuse** and effective **backpressure management**.
