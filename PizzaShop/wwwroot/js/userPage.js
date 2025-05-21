const toggleSidebar = () => {
  const sidebar = document.getElementById("sidebar");
  const overlay = document.getElementById("overlay");
  sidebar.classList.toggle("active");
  overlay.classList.toggle("active");
};

// document.getElementById("overlay").addEventListener("click", toggleSidebar);


document.getElementById('sidebarToggle').addEventListener('click', function () {
  var sidebar = document.getElementById('sidebar');
  var content = document.getElementById('content');
  var overlay = document.getElementById('overlay');

  // Toggle sidebar visibility
  sidebar.classList.toggle('show');
  overlay.classList.toggle('show');

  // Adjust content margin for sidebar visibility
  if (window.innerWidth <= 1200) {
    if (sidebar.classList.contains('show')) {
      content.style.marginLeft = "0"; // Sidebar is visible
    } else {
      content.style.marginLeft = "0"; // Sidebar is hidden
    }
  }
});

// Overlay click to hide sidebar
document.getElementById('sidebarClose').addEventListener('click', function () {
  document.getElementById('sidebar').classList.remove('show');
  document.getElementById('content').style.marginLeft = "0";
  document.getElementById('overlay').classList.remove('show');
});

// Adjust layout on window resize
window.addEventListener('resize', function () {
  var sidebar = document.getElementById('sidebar');
  var content = document.getElementById('content');

  if (window.innerWidth > 1200) {
    sidebar.classList.remove('show'); // Ensure sidebar is visible
    content.style.marginLeft = "300px"; // Default margin for larger screens
  } else {
    content.style.marginLeft = "0"; // Full width on small screens
  }
});

document.addEventListener("DOMContentLoaded", function () {
  const sidebarItems = document.querySelectorAll(".dashboardicon");
  const currentPathSegments = window.location.pathname.toLowerCase().split('/').filter(Boolean); // Get URL segments

  let matchedItem = null;

  sidebarItems.forEach(item => {
    const link = item.querySelector("a");
    if (link) {
      const linkPathSegments = new URL(link.href, window.location.origin).pathname.toLowerCase().split('/').filter(Boolean);

      // Check if the second segment matches
      if (currentPathSegments.length > 1 && linkPathSegments.includes(currentPathSegments[1])) {
        matchedItem = item;
      }
      // If no match, check if the first segment matches
      else if (currentPathSegments.length > 0 && linkPathSegments.includes(currentPathSegments[0]) && !matchedItem) {
        matchedItem = item;
      }
    }
  });

  // Apply active class to the matched sidebar item
  if (matchedItem) {
    matchedItem.classList.add("active");
  }
});

