"use client";

import { useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import { AppBar, AppBarSection, AppBarSpacer, Drawer, DrawerContent } from "@progress/kendo-react-layout";
import { Button } from "@progress/kendo-react-buttons";
import {
  calendarIcon,
  chartColumnClusteredIcon,
  groupIcon,
  menuIcon,
  userIcon,
  type SVGIcon,
} from "@progress/kendo-svg-icons";
import { useMediaQuery } from "@/lib/useMediaQuery";

interface NavItem {
  text: string;
  route: string;
  svgIcon: SVGIcon;
}

const navItems: NavItem[] = [
  { text: "Dashboard", route: "/", svgIcon: chartColumnClusteredIcon },
  { text: "Employees", route: "/employees", svgIcon: userIcon },
  { text: "Departments", route: "/departments", svgIcon: groupIcon },
  { text: "Roster", route: "/roster", svgIcon: calendarIcon },
];

const isActive = (pathname: string, route: string) =>
  route === "/" ? pathname === "/" : pathname === route || pathname.startsWith(`${route}/`);

/**
 * Persistent chrome around every page: a top bar plus a Kendo Drawer for navigation.
 * On wide screens the drawer is a collapsible icon rail that pushes the content; on phones it
 * is hidden until the menu button is pressed and then slides over the page.
 */
export function AppShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const isDesktop = useMediaQuery("(min-width: 992px)");

  const [desktopExpanded, setDesktopExpanded] = useState(true);
  const [mobileOpen, setMobileOpen] = useState(false);
  const expanded = isDesktop ? desktopExpanded : mobileOpen;

  const toggle = () => (isDesktop ? setDesktopExpanded((value) => !value) : setMobileOpen((value) => !value));

  return (
    <div className="app-shell">
      <AppBar positionMode="sticky" themeColor="inverse" className="app-bar">
        <AppBarSection>
          <Button svgIcon={menuIcon} fillMode="flat" aria-label="Toggle navigation" onClick={toggle} />
        </AppBarSection>
        <AppBarSection>
          <span className="app-title">Workforce Portal</span>
        </AppBarSection>
        <AppBarSpacer />
      </AppBar>

      <Drawer
        expanded={expanded}
        mode={isDesktop ? "push" : "overlay"}
        mini={isDesktop}
        width={220}
        miniWidth={56}
        items={navItems.map((item) => ({ ...item, selected: isActive(pathname, item.route) }))}
        onSelect={(event) => {
          router.push(event.itemTarget.route);
          setMobileOpen(false);
        }}
        onOverlayClick={() => setMobileOpen(false)}
      >
        <DrawerContent>
          <main className="app-content">{children}</main>
        </DrawerContent>
      </Drawer>
    </div>
  );
}
