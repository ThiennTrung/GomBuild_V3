# GomBuild V3

Công cụ hỗ trợ gom build & commit dự án SVN.

---

## 1. Setup

- Download bộ cài tại:
  👉 /main/GomBuild_V3/publish
- Chạy file `setup.exe` để cài đặt

---

## 2. Config

Sau khi cài đặt xong:

1. Mở **Task Manager**
2. Tìm tiến trình **GomBuild_V3**
3. Chuột phải → **Open file location**
4. Mở file `GomBuild_V3.exe.config` để chỉnh sửa

---

## 3. Cấu hình quan trọng

- **WokingCopy**:
  - Phải trỏ tới **folder gốc của SVN**
  - **KHÔNG** bao gồm tên dự án con bên trong

✅ Ví dụ đúng:  `<add key="WokingCopy" value="D:\FOLDER_SVN" />`   
❌ Sai: `<add key="WokingCopy" value="D:\FOLDER_SVN\DALIEU" />`


---

## 4. Lưu ý

- Phải **checkout SVN dự án trước khi commit**
- User SVN phải được **phân quyền Add / Commit**
- Đảm bảo working copy ở trạng thái sạch (không conflict)

---

## Support

Nếu gặp lỗi trong quá trình cài đặt hoặc commit, cần kiểm tra:
- Quyền SVN
- Đường dẫn `WokingCopy`
- Trạng thái working copy

