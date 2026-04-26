export default function Input({
  className = "",
  leftIcon = null,
  ...props
}) {
  return (
    <div
      className={`flex items-center rounded-[18px] border border-[#DCE9DE] bg-[#FCFEFC] px-4 py-3.5 transition focus-within:border-[#6FC17A] focus-within:ring-4 focus-within:ring-[#EAF7EC] ${className}`}
    >
      {leftIcon ? <span className="mr-3 text-[#8EA094]">{leftIcon}</span> : null}

      <input
        className="w-full border-none bg-transparent text-[16px] font-medium text-[#2E463A] outline-none placeholder:text-[#92A297]"
        {...props}
      />
    </div>
  );
}