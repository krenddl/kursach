export default function Button({
  children,
  variant = "primary",
  className = "",
  ...props
}) {
  const base =
    "inline-flex items-center justify-center rounded-[18px] px-5 py-3 text-[15px] font-semibold transition disabled:cursor-not-allowed disabled:opacity-60";

  const styles = {
    primary:
      "bg-[#6FC17A] text-white hover:bg-[#5FB06A] shadow-[0_12px_24px_rgba(111,193,122,0.22)]",
    secondary:
      "border border-[#DCE9DE] bg-white text-[#3C5248] hover:bg-[#F7FBF7]",
    ghost:
      "bg-transparent text-[#5D7468] hover:bg-[#EFF7F0]",
    danger:
      "border border-[#F1D4D4] bg-[#FFF4F4] text-[#D87474] hover:bg-[#FFEDED]",
  };

  return (
    <button
      className={`${base} ${styles[variant] || styles.primary} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}