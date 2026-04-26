import { Outlet } from "react-router-dom";
import Sidebar from "./Sidebar";
import Topbar from "./Topbar";

export default function AppLayout() {
  return (
    <div className="min-h-screen bg-[#F4F8F4] p-3 lg:p-6">
      <div className="mx-auto flex min-h-[calc(100vh-24px)] max-w-[1500px] overflow-hidden rounded-[28px] border border-[#E3ECE4] bg-[#FBFDFB] shadow-[0_16px_36px_rgba(111,193,122,0.06)] lg:min-h-[calc(100vh-48px)]">
        <Sidebar />

        <div className="flex min-w-0 flex-1 flex-col">
          <Topbar />
          <main className="flex-1 overflow-auto bg-[#FCFEFC] p-5 lg:p-8">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
}
