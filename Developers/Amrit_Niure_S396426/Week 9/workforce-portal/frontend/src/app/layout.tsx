import type { Metadata } from "next";
import { Geist } from "next/font/google";
import "@progress/kendo-theme-default/dist/all.css";
import "./globals.css";
import { AppShell } from "@/components/AppShell";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: {
    default: "Workforce Portal",
    template: "%s | Workforce Portal",
  },
  description: "Manage employees, departments and rosters with Next.js, Kendo UI and an ASP.NET Core API.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en" className={geistSans.variable}>
      <body>
        <AppShell>{children}</AppShell>
      </body>
    </html>
  );
}
