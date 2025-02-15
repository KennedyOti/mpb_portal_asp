const navLinks = document.querySelectorAll(".nav-link");
const navbarCollapse = document.querySelector(".navbar-collapse");

navLinks.forEach((link) => {
    link.addEventListener("click", () => {
        const bsCollapse = new bootstrap.Collapse(navbarCollapse, {
            toggle: false,
        });
        bsCollapse.hide();
    });
});

// Select the navbar element
const navbar = document.querySelector(".navbar");

// Add a scroll event listener to change the navbar background color
window.addEventListener("scroll", () => {
    // Check if the page has scrolled down
    if (window.scrollY > 0) {
        // Add the 'scrolled' class to change the background color
        navbar.classList.add("scrolled");
    } else {
        // Remove the 'scrolled' class when at the top of the page
        navbar.classList.remove("scrolled");
    }
});

window.onload = function () {
    document.querySelector(".loader-container").style.display = "none";
};

//<!-- gallery section script -->

// Array of image sets
const imageSets = [
    [
        { src: "assets/images/p3.jpg", alt: "" },
        { src: "assets/images/p4.jpg", alt: "" },
        { src: "assets/images/p5.jpg", alt: "" },
        { src: "assets/images/p6.jpg", alt: "" },
        { src: "assets/images/p19.jpg", alt: "" },
        { src: "assets/images/p23.jpg", alt: "" },
    ],
    [
        { src: "assets/images/p7.jpg", alt: "" },
        { src: "assets/images/p8.jpg", alt: "" },
        { src: "assets/images/p9.jpg", alt: "" },
        { src: "assets/images/p10.jpg", alt: "" },
        { src: "assets/images/p19.jpg", alt: "" },
        { src: "assets/images/p12.jpg", alt: "" },
    ],
    [
        { src: "assets/images/p13.jpg", alt: "" },
        { src: "assets/images/p14.jpg", alt: "" },
        { src: "assets/images/p15.jpg", alt: "" },
        { src: "assets/images/p16.jpg", alt: "" },
        { src: "assets/images/p17.jpg", alt: "" },
        { src: "assets/images/p18.jpg", alt: "" },
    ],
];

let currentSet = 0; // Index of the current image set

// Function to load a specific image set
function loadImageSet(index) {
    const galleryItems = document.querySelectorAll(".gallery-item img");
    const selectedSet = imageSets[index];

    // Update each image in the gallery with the new set
    galleryItems.forEach((img, i) => {
        img.src = selectedSet[i].src;
        img.alt = selectedSet[i].alt;
    });
}

// Event listeners for buttons
document.getElementById("prev-btn").addEventListener("click", () => {
    currentSet = (currentSet - 1 + imageSets.length) % imageSets.length;
    loadImageSet(currentSet);
});

document.getElementById("next-btn").addEventListener("click", () => {
    currentSet = (currentSet + 1) % imageSets.length;
    loadImageSet(currentSet);
});

// Load the initial set of images
loadImageSet(currentSet);

// <!-- Add JavaScript to control smooth sliding -->

document.addEventListener("DOMContentLoaded", function () {
    const carousel = document.querySelector("#carouselExampleIndicators");

    // Listen to the 'slide.bs.carousel' event to ensure smooth transition between slides
    carousel.addEventListener("slide.bs.carousel", function (event) {
        // Programmatically controlling the sliding interval and behavior
        const targetSlide = event.relatedTarget;
        const activeSlide = carousel.querySelector(".carousel-item.active");

        // Ensure the slides move like they are attached by synchronizing movement
        if (activeSlide && targetSlide) {
            targetSlide.style.transition = "transform 0.6s ease-in-out";
        }
    });
});

// <!-- JavaScript for click functionality -->

document.addEventListener("DOMContentLoaded", () => {
    const galleryImages = document.querySelectorAll(".gallery-img");

    galleryImages.forEach((img) => {
        img.addEventListener("click", () => {
            // Toggle the 'clicked' class on click to resize to original size
            if (img.classList.contains("clicked")) {
                img.classList.remove("clicked");
                img.style.transform = "scale(2)"; // Restore to hover size when clicked again
            } else {
                img.classList.add("clicked");
                img.style.transform = "scale(1)"; // Display the original size
            }
        });
    });
});
