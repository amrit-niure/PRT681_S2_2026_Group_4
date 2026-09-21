import type { Metadata } from "next";
import { DepartmentsGrid } from "@/components/departments/DepartmentsGrid";
import { serverApi } from "@/lib/api/server";

export const metadata: Metadata = { title: "Departments" };

export const dynamic = "force-dynamic";

export default async function DepartmentsPage() {
  const departments = await serverApi.departments.list();

  return (
    <>
      <div className="page-header">
        <div>
          <h1>Departments</h1>
          <p className="page-subtitle">{departments.length} departments</p>
        </div>
      </div>
      <DepartmentsGrid departments={departments} />
    </>
  );
}
