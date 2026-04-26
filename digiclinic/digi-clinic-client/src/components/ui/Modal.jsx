export default function Modal({
  open,
  title,
  onClose,
  children,
  className = "",
}) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-[#183B2B]/20 px-4 py-6 backdrop-blur-[6px]">
      <div
        className={`w-full max-w-[720px] rounded-[30px] border border-[#D9E7DB] bg-white p-6 shadow-[0_18px_50px_rgba(24,59,43,0.16)] ${className}`}
      >
        <div className="flex items-center justify-between gap-4">
          <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
            {title}
          </div>
          <button
            type="button"
            onClick={onClose}
            className="flex h-11 w-11 items-center justify-center rounded-full border border-[#E3ECE4] bg-[#F8FBF8] text-xl font-semibold text-[#6F8278] transition hover:bg-[#EEF6EF]"
            aria-label="Закрыть"
          >
            ×
          </button>
        </div>

        <div className="mt-5">{children}</div>
      </div>
    </div>
  );
}
